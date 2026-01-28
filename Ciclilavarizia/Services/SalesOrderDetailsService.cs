using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.EntityFrameworkCore;


namespace Ciclilavarizia.Services
{
    public class SalesOrderDetailsService
    {
        private readonly CiclilavariziaDevContext _context;
        public SalesOrderDetailsService(CiclilavariziaDevContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SalesOrderDetail>>> GetDetailsAsync()
        {

            var details = await _context.SalesOrderDetails.ToListAsync();

            return Result<List<SalesOrderDetail>>.Success(details);
        }

        public async Task<Result<List<SalesOrderDetailDto>>> GetDetailAsync(int SalesOrderID)
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

        public async Task<Result<int>> AddSalesDetailsAsync(SalesOrderDetailDto sales, int SalesOrderHeaderID) //riguarda che problema c'era
        {
            var head = await _context.SalesOrderHeaders.FirstOrDefaultAsync(c => c.SalesOrderID == SalesOrderHeaderID);
            if (head == null)
                return Result<int>.Success(0);
            else if (sales == null)
                return Result<int>.Failure("no data to add");

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == sales.ProductID);
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
            await _context.SalesOrderDetails.AddAsync(details);

            await _context.SaveChangesAsync();

            return Result<int>.Success(details.SalesOrderDetailID);
        }
        public async Task<Result<int>> UpdateSalesDetailsAsync(SalesOrderDetailDto detail, int SalesOrderDetailID)
        {

            var detailed = await _context.SalesOrderDetails
                            .FirstOrDefaultAsync(c => c.SalesOrderDetailID == SalesOrderDetailID);

            if (detailed == null)
                return Result<int>.Success(0);
            if (detail == null)
                return Result<int>.Failure("no data to update");

            if (detail.ProductID > 0)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == detail.ProductID);
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

        public async Task<Result<bool>> DeleteSalesDetailsAsync(int SalesOrderDetailsID)
        {

            var details = await _context.SalesOrderDetails
                            .FirstOrDefaultAsync(p => p.SalesOrderDetailID == SalesOrderDetailsID);

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
