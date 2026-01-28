using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderHeaderController : ControllerBase
    {
        private readonly SalesOrderHeaderService _service;
        public SalesOrderHeaderController(SalesOrderHeaderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalesOrderHeader>>> AllHeaders()
        {
            var headers = await _service.GetHeadersAsync();
            if (!headers.IsSuccess)
                return NotFound(headers.ErrorMessage);

            return Ok(headers.Value);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetMyHeader(int customerId)
        {

            var header = await _service.GetHeaderAsync(customerId);
            if (!header.IsSuccess)
            {
                return NotFound(header.ErrorMessage);
            }
            return Ok(header.Value);


        }

        [HttpPost]
        public async Task<ActionResult<SalesOrderHeader>> AddSalesHeader([FromBody] SalesOrderHeaderDto sales)
        {

            var header = await _service.AddSalesHeaderAsync(sales);
            if (!header.IsSuccess)
            {
                return BadRequest(header.ErrorMessage);
            }
            else if (header.Value == 0)
            {
                return NotFound();
            }

            return CreatedAtAction("GetMyHeader", header.Value);
        }


        [HttpPut("{salesOrderId}")]
        public async Task<ActionResult<SalesOrderHeader>> ModifySalesHeader([FromBody] SalesOrderHeaderDto header, int salesOrderId)
        {

            var headered = await _service.UpdateSalesHeaderAsync(header, salesOrderId);

            if (!headered.IsSuccess)
            {
                return BadRequest(headered.ErrorMessage);
            }
            else if (headered.Value == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{salesOrderId}")]
        public async Task<ActionResult> DeleteSalesHeader(int salesOrderId)
        {

            var header = await _service.DeleteSalesHeaderAsync(salesOrderId);

            if (!header.IsSuccess)
                return NotFound();
            if (!header.Value)
                return Problem();

            return NoContent();
        }
    }
}