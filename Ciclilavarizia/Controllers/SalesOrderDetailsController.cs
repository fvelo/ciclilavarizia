using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SalesOrderDetailsController : ControllerBase
    {
        private readonly CiclilavariziaDevContext _context;
        private readonly SalesOrderService _service;
        public SalesOrderDetailsController(CiclilavariziaDevContext context, SalesOrderService salesOrderService)
        {
            _context = context;
            _service = salesOrderService;
        }

        [HttpGet()]
        public async Task<ActionResult<List<SalesOrderDetail>>> AllDetails()
        {
            return await _service.AllDetails();
        }

        [HttpGet("{SalesOrderID}")]
        public async Task<ActionResult<List<SalesOrderDetailDto>>> GetMyDetails(int SalesOrderID)
        {

            var details = await _service.GetMyDetails(SalesOrderID);
            if (details.Count == 0)
            {
                return BadRequest("no detail found with this ID");
            }

            return details;
        }

        [HttpPost("{SalesOrderHeaderID}")]
        public async Task<ActionResult<SalesOrderDetail>> AddSalesDetails([FromBody] SalesOrderDetailDto sales, int SalesOrderHeaderID)
        {

            var details = await _service.AddSalesDetails(sales, SalesOrderHeaderID);
            if(!details)
            {
                return BadRequest(" no Header found");
            }

            return Ok(sales);
        }

        [HttpPut("{SalesOrderDetailID}")]
        public async Task<ActionResult<SalesOrderDetail>> ModifySalesDetails([FromBody] SalesOrderDetailDto detail, int SalesOrderDetailID)
        {

            var detailed = await _service.ModifySalesDetails(detail, SalesOrderDetailID);
            if(!detailed)
            {
                return BadRequest(" no match found");
            }

            return NoContent();
        }

        [HttpDelete("{SalesOrderDetailsID}")]
        public async Task<ActionResult> DeleteSalesDetails(int SalesOrderDetailsID)
        {

            var details = await _service.DeleteSalesDetails(SalesOrderDetailsID);
            if (!details)
            {
                return BadRequest(" no match found");
            }

            return NoContent();
        }

    }
}

