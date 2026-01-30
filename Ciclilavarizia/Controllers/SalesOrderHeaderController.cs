using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderHeaderController : ControllerBase
    {
        private readonly ISalesOrderHeaderService _service;

        public SalesOrderHeaderController(ISalesOrderHeaderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetAllHeaders()
        {
            var result = await _service.GetHeadersAsync();
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return Ok(result.Value);
        }

        // GET: api/SalesOrderHeader/50123
        [HttpGet("{customerId}")]
        public async Task<ActionResult<SalesOrderHeaderDto>> GetMyHeaders(int customerId)
        {
            var result = await _service.GetHeaderByCustomerIdAsync(customerId);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpGet("single/{orderHeaderId}")]
        public async Task<ActionResult<SalesOrderHeaderDto>> GetHeader(int orderHeaderId)
        {
            var result = await _service.GetHeaderByIdAsync(orderHeaderId);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return Ok(result.Value);
        }

        // TODO: this does not work, make it work
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder([FromBody] SalesOrderHeaderCommandDto command)
        {
            var result = await _service.CreateOrderAsync(command);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            // Returns 201 Created with the URI to fetch the new resource
            return CreatedAtAction(nameof(GetMyHeaders), new { id = result.Value }, result.Value);
        }

        [HttpDelete("{orderHeaderId}")]
        public async Task<IActionResult> DeleteOrder(int orderHeaderId)
        {
            var result = await _service.DeleteOrderAsync(orderHeaderId);
            return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrder(SalesOrderHeaderCommandDto command)
        {
            var result = await _service.UpdateOrderAsync(command);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }
    }
}