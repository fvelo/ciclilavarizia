using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace Ciclilavarizia.Services
{
    public class SalesOrderService
    {
        private readonly CiclilavariziaDevContext _context;
        public SalesOrderService(CiclilavariziaDevContext context)
        {
            _context = context;
        }

        //headers services
        public async Task<Result<List<SalesOrderHeader>>> AllHeaders()
        {
            var headers = await _context.SalesOrderHeaders
                .ToListAsync();
            if(headers == null || headers.Count == 0)
            {
                return Result<List<SalesOrderHeader>>.Failure("No headers found");
            }
            return Result<List<SalesOrderHeader>>.Success(headers);
        }

        public async Task<Result<List<SalesOrderHeaderDto>>> GetMyHeader(int CustomerID)
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
                .Where(c => c.CustomerID == CustomerID)
                .ToListAsync();
            if(header == null || header.Count == 0)
            {
                return Result<List<SalesOrderHeaderDto>>.Failure("No headers found for this customer");
            }

                return Result<List<SalesOrderHeaderDto>>.Success(header);
        }

        public async Task<Result<int>> AddSalesHeader(SalesOrderHeaderDto sales)
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
            _context.SalesOrderHeaders.Add(header);
            await _context.SaveChangesAsync();

            return Result<int>.Success(header.SalesOrderID);
        }

        public async Task<Result<int>> ModifySalesHeader(SalesOrderHeaderDto header, int SalesOrderID)
        {

            var headered = _context.SalesOrderHeaders
                .FirstOrDefault(c => c.SalesOrderID == SalesOrderID);
            if (headered == null)
                return Result<int>.Success(0);
            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerID == header.CustomerID);
            if (customer == null)
                return Result<int>.Success(0);

            if(header == null)
            {
                return Result<int>.Failure("no data to update");
            }

            if (header.Status > header.Status)
            {
                headered.Status = header.Status;
            }
            if (header.DueDate > DateTime.MinValue)
            {
                headered.DueDate = header.DueDate;
            }
            if (header.OrderDate > DateTime.MinValue)
            {
                headered.OrderDate = header.OrderDate;
            }
            if (header.ShipDate > DateTime.MinValue)
            {
                headered.ShipDate = header.ShipDate;
            }
            if (header.ShipMethod != null)
            {
                headered.ShipMethod = header.ShipMethod;
            }
            if (header.SubTotal > 0)
            {
                headered.SubTotal = header.SubTotal;
            }

            if (header.TaxAmt > 0)
            {
                headered.TaxAmt = header.TaxAmt;
            }

            if (header.Freight > 0)
            {
                headered.Freight = header.Freight;
            }

            if (header.Comment != null)
            {
                headered.Comment = header.Comment;
            }

            _context.Entry(headered).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Result<int>.Success(header.SalesOrderID);
        }

        public async Task<Result<bool>> DeleteSalesHeader(int SalesOrderID)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var header = _context.SalesOrderHeaders
                            .FirstOrDefault(p => p.SalesOrderID == SalesOrderID);
                var details = _context.SalesOrderDetails.FirstOrDefault(c => c.SalesOrderID == SalesOrderID);

                if (header == null)
                    return Result<bool>.Failure("no header found");

                if (details != null)
                {
                    _context.Remove(details);
                }

                _context.Remove(header);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<bool>.Success(false);
            }
        }


        // details services
        public async Task<Result<List<SalesOrderDetail>>> AllDetails()
        {

            var details = await _context.SalesOrderDetails.ToListAsync();

            return Result<List<SalesOrderDetail>>.Success(details);
        }


        public async Task<Result<List<SalesOrderDetailDto>>> GetMyDetails(int SalesOrderID)
        {

            var details = await _context.SalesOrderDetails
                .Where(c => c.SalesOrderID == SalesOrderID)
                .Select(a => new SalesOrderDetailDto
                {
                    SalesOrderId = a.SalesOrderID,
                    ProductID = a.ProductID,
                    OrderQty = a.OrderQty,
                    UnitPrice = a.UnitPrice,
                    UnitPriceDiscount = a.UnitPriceDiscount,
                })
                .ToListAsync();

            return Result<List<SalesOrderDetailDto>>.Success(details);
        }
        public async Task<Result<int>> AddSalesDetails(SalesOrderDetailDto sales, int SalesOrderHeaderID)
        {
            var head = _context.SalesOrderHeaders.FirstOrDefault(c => c.SalesOrderID == SalesOrderHeaderID);
            if(head == null)
                return Result<int>.Success(0);
            else if (sales == null)
                return Result<int>.Failure("no data to add");

            var product = _context.Products.FirstOrDefault(p => p.ProductID == sales.ProductID);
            if (product == null)
                return Result<int>.Success(0);


            var details = new SalesOrderDetail
                {
                    SalesOrderID = SalesOrderHeaderID,
                    OrderQty = sales.OrderQty,
                    ProductID = sales.ProductID,
                    UnitPrice = product.ListPrice,
                    UnitPriceDiscount = sales.UnitPriceDiscount

                };
            _context.SalesOrderDetails.Add(details);

            await _context.SaveChangesAsync();

            return Result<int>.Success(details.SalesOrderDetailID);
        }
        public async Task<Result<int>> ModifySalesDetails(SalesOrderDetailDto detail, int SalesOrderDetailID)
        {

            var detailed = _context.SalesOrderDetails
                            .FirstOrDefault(c => c.SalesOrderDetailID == SalesOrderDetailID);

            if (detailed == null)
                return Result<int>.Success(0);
            if (detail == null)
                return Result<int>.Failure("no data to update");

            if (detail.ProductID > 0)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductID == detail.ProductID);
                if (product == null)
                    return Result<int>.Success(0);

            
                detailed.ProductID = detail.ProductID;
                detailed.UnitPrice = product.ListPrice;
            }

            if (detail.OrderQty > 0)
            {
                detailed.OrderQty = detail.OrderQty;
            }

            if (detail.UnitPriceDiscount > 0)
            {
                detailed.UnitPriceDiscount = detail.UnitPriceDiscount;
            }

            _context.Entry(detailed).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result<int>.Success(detailed.SalesOrderDetailID);
        }

        public async Task<Result<bool>> DeleteSalesDetails(int SalesOrderDetailsID)
        {

            var details = _context.SalesOrderDetails
                            .FirstOrDefault(p => p.SalesOrderDetailID == SalesOrderDetailsID);

            if (details == null)
            {
                return Result<bool>.Success(false);
            }

            _context.Remove(details);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
