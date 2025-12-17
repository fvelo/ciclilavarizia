using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Ciclilavarizia.Controllers
{
    //needs error control and the accessibility (aka the authorization to which type of access)
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class OrderController1 : ControllerBase
    {
        private readonly CiclilavariziaDevContext _context;
        public OrderController1(CiclilavariziaDevContext context)
        {
            _context = context;
        }

        [HttpGet("Headers")]
        public async Task<ActionResult<List<SalesOrderHeader>>> AllHeaders()
        {
            try
            {
                var headers = await _context.SalesOrderHeaders
                    .ToListAsync();
                return headers;
            }

            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Headers/{CustomerID}")]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetMyHeader(int CustomerID)
        {
            try
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
                if (header.Count == 0)
                {
                    return BadRequest("no header found for this customer ID");
                }
                return header;
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Details")]
        public async Task<ActionResult<List<SalesOrderDetail>>> AllDetails()
        {
            try
            {
                var details = await _context.SalesOrderDetails.ToListAsync();

                return details;
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Details/{SalesOrderID}")]
        public async Task<ActionResult<List<SalesOrderDetailDto>>> GetMyDetails(int SalesOrderID)
        {
            try
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
                if (details.Count == 0)
                {
                    return BadRequest("no detail found with this ID");
                }
                
                return details;
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
            
        }

        [HttpPost("Header")]
        public async Task<ActionResult<SalesOrderHeader>> AddSalesHeader([FromBody] SalesOrderHeaderDto sales)
        {
            try
            {
                var address = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.CustomerID == sales.CustomerID);
                if (address == null)
                {
                    return BadRequest("No address found");
                }

            
                var header = new SalesOrderHeader
                {
                    OrderDate = sales.OrderDate,
                    DueDate = sales.DueDate,
                    ShipDate = sales.ShipDate,
                    Status = 1,
                    OnlineOrderFlag = false, //to review
                    PurchaseOrderNumber = (sales.PurchaseOrderNumber) ?? null,
                    CustomerID = sales.CustomerID,
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
            }
            catch (Exception e) 
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPost("Details/{SalesOrderHeaderID}")]
        public async Task<ActionResult<SalesOrderDetail>> AddSalesDetails([FromBody] SalesOrderDetailDto sales, int SalesOrderHeaderID)
        {
            try
            {
                _context.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    SalesOrderID = SalesOrderHeaderID,
                    OrderQty = sales.OrderQty,
                    ProductID = sales.ProductID,
                    UnitPrice = sales.UnitPrice,
                    UnitPriceDiscount = sales.UnitPriceDiscount

                });
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(sales);
        }

        [HttpPut("Header{SalesOrderID}")]
        public async Task<ActionResult<SalesOrderHeader>> ModifySalesHeader([FromBody]SalesOrderHeaderDto header, int SalesOrderID) 
        {
            try
            {
                var headered = _context.SalesOrderHeaders.FirstOrDefault(c=> c.SalesOrderID == SalesOrderID);
            
                if (headered == null)
                {
                    return BadRequest("no header found");
                }
                if (header.Status != 0)
                {
                    headered.Status = header.Status;
                }
                if (header.DueDate != DateTime.MinValue)
                {
                    headered.DueDate = header.DueDate;
                }
                if (header.OrderDate != DateTime.MinValue)
                {
                    headered.OrderDate = header.OrderDate;
                }
                if (header.ShipDate != DateTime.MinValue)
                {
                    headered.ShipDate = header.ShipDate;
                }
                if (header.ShipMethod != null)
                {
                    headered.ShipMethod = header.ShipMethod;
                }
                if (header.SubTotal != 0)
                {
                    headered.SubTotal = header.SubTotal;
                }

                if (header.TaxAmt != 0)
                {
                    headered.TaxAmt = header.TaxAmt;
                }

                if (header.Freight != 0)
                {
                    headered.Freight = header.Freight;
                }

                if (header.Comment != null)
                {
                    headered.Comment = header.Comment;
                }

                _context.Entry(headered).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(header);
        }

        [HttpPut("Details{SalesOrderDetailID}")]
        public async Task<ActionResult<SalesOrderDetail>> ModifySalesDetails([FromBody] SalesOrderDetailDto detail, int SalesOrderDetailID)
        {
            try
            {
                var detailed = _context.SalesOrderDetails
                                .FirstOrDefault(c => c.SalesOrderDetailID == SalesOrderDetailID);

                if (detailed == null)
                    return BadRequest("0 results for this details id");

                if (detail.ProductID != 0)
                {
                    detailed.ProductID = detail.ProductID;
                }

                if (detail.UnitPrice != 0)
                {
                    detailed.UnitPrice = detail.UnitPrice;
                }

                if (detail.OrderQty != 0)
                {
                    detailed.OrderQty = detail.OrderQty;
                }

                if (detail.UnitPriceDiscount != 0)
                {
                    detailed.UnitPriceDiscount = detail.UnitPriceDiscount;
                }

                _context.Entry(detailed).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok(detail);
        }

        [HttpDelete("Headers/{SalesOrderID}")]
        public async Task<ActionResult> DeleteSalesHeader(int SalesOrderID) 
        {
            try
            {
                var header = _context.SalesOrderHeaders
                            .FirstOrDefault(p => p.SalesOrderID == SalesOrderID);
                var details = _context.SalesOrderDetails.FirstOrDefault(c => c.SalesOrderID == SalesOrderID);

                if (header == null || details == null)
                    return BadRequest(" no records found ");

                _context.Remove(details);
                _context.Remove(header);
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return NoContent();
        }

        [HttpDelete("Details/{SalesOrderDetailsID}")]
        public async Task<ActionResult> DeleteSalesDetails(int SalesOrderDetailID)
        {
            try
            {
                var details = _context.SalesOrderDetails
                                .FirstOrDefault(p => p.SalesOrderDetailID == SalesOrderDetailID);

                if (details == null)
                {
                    return BadRequest(" no match found");
                }

                _context.Remove(details);
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return NoContent();
        }

    }
}

