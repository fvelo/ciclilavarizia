using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public interface ISalesOrderHeaderService
    {
        Task<Result<IEnumerable<SalesOrderHeaderDto>>> GetHeadersAsync();
        Task<Result<IEnumerable<SalesOrderHeaderDto>>> GetHeaderByCustomerIdAsync(int customerId);
        Task<Result<SalesOrderHeaderDto>> GetHeaderByIdAsync(int salesOrderId);
        Task<Result<int>> CreateOrderAsync(SalesOrderHeaderCommandDto command);
        Task<Result<bool>> DeleteOrderAsync(int salesOrderId);
        Task<Result<int>> UpdateOrderAsync(SalesOrderHeaderCommandDto command);
        Task UpdateHeaderTotalsAsync(int salesOrderId); // Internal helper logic

    }

    public class SalesOrderHeaderService : ISalesOrderHeaderService
    {
        private readonly CiclilavariziaDevContext _context;
        private const decimal DefaultTaxRate = 0.08m;

        public SalesOrderHeaderService(CiclilavariziaDevContext context)
        {
            _context = context;
        }
        public async Task<Result<IEnumerable<SalesOrderHeaderDto>>> GetHeadersAsync()
        {
            var query = _context.SalesOrderHeaders.AsNoTracking();

            var headers = await query.Select(h => MapToDto(h)).ToListAsync();
            return Result<IEnumerable<SalesOrderHeaderDto>>.Success(headers);
        }

        public async Task<Result<IEnumerable<SalesOrderHeaderDto>>> GetHeaderByCustomerIdAsync(int customerId)
        {
            var query = _context.SalesOrderHeaders
                .AsNoTracking()
                .Where(h => h.CustomerID == customerId);

            var headers = await query.Select(h => MapToDto(h)).ToListAsync();
            return Result<IEnumerable<SalesOrderHeaderDto>>.Success(headers);
        }

        public async Task<Result<SalesOrderHeaderDto>> GetHeaderByIdAsync(int salesOrderId)
        {
            var header = await _context.SalesOrderHeaders
                .AsNoTracking()
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(h => h.SalesOrderID == salesOrderId);

            if (header == null) return Result<SalesOrderHeaderDto>.Failure("Order not found.");

            return Result<SalesOrderHeaderDto>.Success(MapToDto(header));
        }

        public async Task<Result<int>> CreateOrderAsync(SalesOrderHeaderCommandDto command)
        {
            // Validate Customer and Address
            var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.CustomerID == command.CustomerId);

            if (address == null) return Result<int>.Failure("Valid shipping address not found for customer.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var header = new SalesOrderHeader
                {
                    CustomerID = command.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    DueDate = command.DueDate,
                    ShipMethod = command.ShipMethod,
                    PurchaseOrderNumber = command.PurchaseOrderNumber,
                    Comment = command.Comment,
                    ShipToAddressID = address.AddressID,
                    BillToAddressID = address.AddressID,
                    Status = 1, // Ordered
                    OnlineOrderFlag = true,
                    Freight = 0, // Business logic: calculation or flat rate
                };

                foreach (var detailDto in command.SalesOrderDetails)
                {
                    var product = await _context.Products.FindAsync(detailDto.ProductId);
                    if (product == null) continue;

                    header.SalesOrderDetails.Add(new SalesOrderDetail
                    {
                        ProductID = product.ProductID,
                        OrderQty = detailDto.OrderQty,
                        UnitPrice = product.ListPrice,
                        UnitPriceDiscount = detailDto.UnitPriceDiscount,
                        LineTotal = (product.ListPrice * (1 - detailDto.UnitPriceDiscount)) * detailDto.OrderQty
                    });
                }

                // Initial calculation before saving
                header.SubTotal = header.SalesOrderDetails.Sum(d => d.LineTotal);
                header.TaxAmt = header.SubTotal * DefaultTaxRate;
                header.TotalDue = header.SubTotal + header.TaxAmt + header.Freight;

                _context.SalesOrderHeaders.Add(header);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<int>.Success(header.SalesOrderID);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Result<bool>> DeleteOrderAsync(int salesOrderId)
        {
            var header = await _context.SalesOrderHeaders
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(h => h.SalesOrderID == salesOrderId);

            if (header == null) return Result<bool>.Failure("Order not found.");

            _context.SalesOrderHeaders.Remove(header); // Cascading delete handled by EF or DB
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> UpdateOrderAsync(SalesOrderHeaderCommandDto command)
        {
            // assuming that an object of HeaderOrderDto get backs and there is a List of details, details will be handled this way:
            // if they previously exited in th details list and now they are not there no more, they will be deleted
            // if they previouslly existed and are now different they will be modified
            // if they previouslly does not exists this is not possible it will return a badrequest.

            var existingOrder = await _context.SalesOrderHeaders
               .Include(h => h.SalesOrderDetails)
               .FirstOrDefaultAsync(h => h.SalesOrderID == command.SalesOrderId);

            if (existingOrder == null) Result<int>.Failure("Order not found!");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                existingOrder!.DueDate = command.DueDate;
                existingOrder!.ShipMethod = command.ShipMethod;
                existingOrder!.PurchaseOrderNumber = command.PurchaseOrderNumber;
                existingOrder!.Comment = command.Comment;

                var existingOrderDetails = existingOrder.SalesOrderDetails;
                var incomingOrderDetails = command.SalesOrderDetails;

                var incomingIds = incomingOrderDetails
                    .Select(o => o.ProductId)
                    .ToHashSet();

                // check details present in the old list and not in the new -> delete them
                var orderDetailToDelete = existingOrderDetails
                    .Where(od => !incomingIds.Contains(od.ProductID))
                    .ToList();

                foreach (var incomingOrderDetail in incomingOrderDetails)
                {
                    var existingOrderDetailEntity = existingOrderDetails
                        .FirstOrDefault(od => od.ProductID == incomingOrderDetail.ProductId);

                    if (existingOrderDetailEntity == null)
                    {
                        return Result<int>.Failure($"Product ID {incomingOrderDetail.ProductId} is not part of the original order. Addition is forbidden during update.");
                    }

                    existingOrderDetailEntity.OrderQty = incomingOrderDetail.OrderQty;
                    existingOrderDetailEntity.UnitPriceDiscount = incomingOrderDetail.UnitPriceDiscount;
                    existingOrderDetailEntity.LineTotal = (existingOrderDetailEntity.UnitPrice * (1 - existingOrderDetailEntity.UnitPriceDiscount)) * existingOrderDetailEntity.OrderQty;
                }

                // remove details there are not in the new list but are in the old one
                foreach (var detailToRemove in orderDetailToDelete)
                    existingOrder.SalesOrderDetails.Remove(detailToRemove);

                existingOrder.SubTotal = existingOrder.SalesOrderDetails.Sum(d => d.LineTotal);
                existingOrder.TaxAmt = existingOrder.SubTotal * DefaultTaxRate;
                existingOrder.TotalDue = existingOrder.SubTotal + existingOrder.TaxAmt + existingOrder.Freight;


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<int>.Success(existingOrder.SalesOrderID);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // this will make the exeption bubble to the GEH
            }
        }

        public async Task UpdateHeaderTotalsAsync(int salesOrderId)
        {
            var header = await _context.SalesOrderHeaders
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(h => h.SalesOrderID == salesOrderId);

            if (header != null)
            {
                header.SubTotal = header.SalesOrderDetails.Sum(d => d.LineTotal);
                header.TaxAmt = header.SubTotal * DefaultTaxRate;
                header.TotalDue = header.SubTotal + header.TaxAmt + header.Freight;
                await _context.SaveChangesAsync();
            }
        }

        private static SalesOrderHeaderDto MapToDto(SalesOrderHeader h) => new(
            h.SalesOrderID, h.CustomerID, h.OrderDate, h.Status, h.PurchaseOrderNumber,
            h.SubTotal, h.TaxAmt, h.Freight, h.TotalDue,
            h.SalesOrderDetails.Select(d => new SalesOrderDetailDto(
                d.SalesOrderDetailID, d.ProductID, d.OrderQty, d.UnitPrice, d.UnitPriceDiscount, d.LineTotal
            )).ToList()
        );
    }
}