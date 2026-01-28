using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class SalesOrderHeaderService
    {
        private readonly CiclilavariziaDevContext _context;
        public SalesOrderHeaderService(CiclilavariziaDevContext context)
        {
            _context = context;
        }
        public async Task<Result<List<SalesOrderHeader>>> GetHeadersAsync()
        {
            var headers = await _context.SalesOrderHeaders
                .ToListAsync();
            if (headers == null || headers.Count == 0)
            {
                return Result<List<SalesOrderHeader>>.Failure("No headers found");
            }
            return Result<List<SalesOrderHeader>>.Success(headers);
        }

        public async Task<Result<List<SalesOrderHeaderDto>>> GetHeaderAsync(int customerId)
        {

            var header = await _context.SalesOrderHeaders
                .AsNoTracking()
                .Select(a => new SalesOrderHeaderDto
                {
                    CustomerID = a.CustomerID,
                    DueDate = a.DueDate,
                    ShipMethod = a.ShipMethod,
                    ShipDate = a.ShipDate,
                    Status = a.Status,
                    PurchaseOrderNumber = a.PurchaseOrderNumber,
                    OrderDate = a.OrderDate,
                    Freight = a.Freight,
                    SubTotal = a.SubTotal,
                    TaxAmt = a.TaxAmt,
                    Comment = a.Comment,
                    SalesOrderDetails = a.SalesOrderDetails
                                        .Select(d => new SalesOrderDetailDto
                                        {
                                            SalesOrderId = d.SalesOrderID,
                                            ProductID = d.ProductID,
                                            OrderQty = d.OrderQty,
                                            UnitPrice = d.UnitPrice,
                                            UnitPriceDiscount = d.UnitPriceDiscount,
                                            LineTotal = d.UnitPrice * (1 - d.UnitPriceDiscount) * d.OrderQty
                                        })
                                        .ToList()

                })
                .Where(c => c.CustomerID == customerId)
                .ToListAsync();
            if (header == null || header.Count == 0)
            {
                return Result<List<SalesOrderHeaderDto>>.Failure("No headers found for this customer");
            }

            return Result<List<SalesOrderHeaderDto>>.Success(header);
        }

        public async Task<Result<int>> AddSalesHeaderAsync(SalesOrderHeaderDto sales)
        {

            var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.CustomerID == sales.CustomerID);
            if (address == null)
                return Result<int>.Failure("the address id does not match the customer");


            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerID == sales.CustomerID);
            if (customer == null)
                return Result<int>.Success(0);

            var header = new SalesOrderHeader
            {
                OrderDate = sales.OrderDate,
                DueDate = sales.DueDate,
                ShipDate = sales.ShipDate,
                Status = 1,
                OnlineOrderFlag = true,
                PurchaseOrderNumber = (sales.PurchaseOrderNumber) ?? null,
                CustomerID = customer.CustomerID,
                ShipToAddressID = address.AddressID,
                BillToAddressID = address.AddressID,
                ShipMethod = sales.ShipMethod,
                SubTotal = sales.SubTotal,
                TaxAmt = sales.TaxAmt,
                Freight = sales.Freight,
                TotalDue = sales.SubTotal + sales.TaxAmt + sales.Freight,
                Comment = sales.Comment,
                SalesOrderDetails = []
            };

            if (sales.SalesOrderDetails != null)
            {
                foreach (var prod in sales.SalesOrderDetails)
                {
                    header.SalesOrderDetails.Add(new SalesOrderDetail
                    {
                        SalesOrderID = header.SalesOrderID,
                        ProductID = prod.ProductID,
                        UnitPrice = prod.UnitPrice,
                        OrderQty = prod.OrderQty,
                        LineTotal = prod.LineTotal
                    });
                }
            }
            await _context.SalesOrderHeaders.AddAsync(header);
            await _context.SaveChangesAsync();

            return Result<int>.Success(header.SalesOrderID);
        }

        public async Task<Result<int>> UpdateSalesHeaderAsync(SalesOrderHeaderDto incomingHeader, int SalesOrderID)
        {
            var existingHeader = await _context.SalesOrderHeaders
                .FirstOrDefaultAsync(c => c.SalesOrderID == SalesOrderID);

            if (existingHeader == null) return Result<int>.Success(0); // will return NotFound()

            var customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerID == incomingHeader.CustomerID);

            if (!customerExists) return Result<int>.Success(0); // will return NotFound()

            if (incomingHeader.Status > 0 && incomingHeader.Status != existingHeader.Status)
            {
                existingHeader.Status = incomingHeader.Status;
            }

            if (incomingHeader.DueDate > DateTime.MinValue) existingHeader.DueDate = incomingHeader.DueDate;
            if (incomingHeader.OrderDate > DateTime.MinValue) existingHeader.OrderDate = incomingHeader.OrderDate;
            if (incomingHeader.ShipDate > DateTime.MinValue) existingHeader.ShipDate = incomingHeader.ShipDate;

            if (incomingHeader.SubTotal > 0) existingHeader.SubTotal = incomingHeader.SubTotal;
            if (incomingHeader.TaxAmt > 0) existingHeader.TaxAmt = incomingHeader.TaxAmt;
            if (incomingHeader.Freight > 0) existingHeader.Freight = incomingHeader.Freight;

            existingHeader.TotalDue = existingHeader.SubTotal + existingHeader.TaxAmt + existingHeader.Freight;

            if (!string.IsNullOrEmpty(incomingHeader.Comment)) existingHeader.Comment = incomingHeader.Comment;

            _context.Entry(existingHeader).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result<int>.Success(existingHeader.SalesOrderID);
        }

        public async Task<Result<bool>> DeleteSalesHeaderAsync(int SalesOrderID)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var header = await _context.SalesOrderHeaders
                    .FirstOrDefaultAsync(p => p.SalesOrderID == SalesOrderID);

                if (header == null) return Result<bool>.Failure("Header not found");

                var details = await _context.SalesOrderDetails
                    .Where(c => c.SalesOrderID == SalesOrderID)
                    .ToListAsync();

                if (details.Any())
                {
                    _context.SalesOrderDetails.RemoveRange(details);
                }

                _context.SalesOrderHeaders.Remove(header);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<bool>.Failure($"Delete failed");
            }
        }


    }
}
