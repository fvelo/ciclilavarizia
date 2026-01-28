using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderDetailsController : ControllerBase
    {
        private readonly SalesOrderDetailsService _service;
        public SalesOrderDetailsController(SalesOrderDetailsService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<ActionResult<List<SalesOrderDetail>>> AllDetails()
        {
            var details = await _service.GetDetailsAsync();
            return details.Value.Count > 0
                ? Ok(details.Value)
                : NotFound();
        }

        [HttpGet("{salesOrderId}")]
        public async Task<ActionResult<List<SalesOrderDetailDto>>> GetMyDetails(int salesOrderId)
        {

            var details = await _service.GetDetailAsync(salesOrderId);
            if (details.Value == null || details.Value.Count <= 0)
            {
                return NotFound();
            }

            return Ok(details.Value);
        }

        [HttpPost("{salesOrderHeaderId}")]
        public async Task<ActionResult<SalesOrderDetail>> AddSalesDetails([FromBody] SalesOrderDetailDto sales, int salesOrderHeaderId)
        {

            var details = await _service.AddSalesDetailsAsync(sales, salesOrderHeaderId);
            if (!details.IsSuccess)
            {
                return BadRequest(details.ErrorMessage);
            }
            else if (details.Value == 0)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetMyDetails), new { id = details.Value });
        }

        [HttpPut("{salesOrderDetailsId}")]
        public async Task<ActionResult<SalesOrderDetail>> ModifySalesDetails([FromBody] SalesOrderDetailDto detail, int salesOrderDetailsId)
        {

            var detailed = await _service.UpdateSalesDetailsAsync(detail, salesOrderDetailsId);
            if (!detailed.IsSuccess)
            {
                return BadRequest(detailed.ErrorMessage);
            }
            else if (detailed.Value == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{salesOrderDetailsId}")]
        public async Task<ActionResult> DeleteSalesDetails(int salesOrderDetailsId)
        {

            var details = await _service.DeleteSalesDetailsAsync(salesOrderDetailsId);
            if (!details.Value)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}

