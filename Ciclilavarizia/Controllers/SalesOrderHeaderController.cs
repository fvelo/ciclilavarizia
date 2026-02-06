using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Retrieves all sales order headers in the system.
        /// </summary>
        /// <returns>A list of sales order headers.</returns>
        /// <response code="200">Returns the list of orders.</response>
        /// <response code="404">If no orders are found.</response>
        /// <response code="401">The user is not authorized.</response>
        [HttpGet]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetAllHeaders()
        {
            var result = await _service.GetHeadersAsync();
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves all orders belonging to a specific customer.
        /// </summary>
        /// <param name="customerId">The customer's unique identifier.</param>
        /// <returns>A list of sales orders for the customer.</returns>
        /// <response code="200">Returns the customer's orders.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="404">If no orders are found for this customer.</response>
        // GET: api/SalesOrderHeader/50123
        [HttpGet("{customerId}")]
        [Authorize("UserPolicy")]
        public async Task<ActionResult<SalesOrderHeaderDto>> GetMyHeaders(int customerId)
        {
            var result = await _service.GetHeadersByCustomerIdAsync(customerId);
            if (!result.IsSuccess) return NotFound();

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific sales order by its unique ID.
        /// </summary>
        /// <param name="salesOrderId">The ID of the sales order header.</param>
        /// <returns>The requested sales order header details.</returns>
        /// <response code="200">Returns the requested order.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="404">If the order header does not exist.</response>
        [HttpGet("single/{salesOrderId}")]
        [Authorize("UserPolicy")]
        public async Task<ActionResult<SalesOrderHeaderDto>> GetHeader(int salesOrderId)
        {
            var result = await _service.GetHeaderByIdAsync(salesOrderId);
            if (!result.IsSuccess) return NotFound();

            return Ok(result.Value);
        }

        /// <summary>
        /// Submits a new sales order with multiple line items.
        /// </summary>
        /// <param name="command">The order details and item list.</param>
        /// <returns>The ID of the newly created order.</returns>
        /// <response code="201">Order created successfully; returns the order ID.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="400">If the order data is invalid or calculations fail.</response>
        [HttpPost]
        [Authorize("UserPolicy")]
        public async Task<ActionResult<int>> CreateOrder([FromBody] SalesOrderHeaderCommandDto command)
        {
            var result = await _service.CreateOrderAsync(command);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            // Returns 201 Created with the URI to fetch the new resource
            return CreatedAtAction(nameof(GetMyHeaders), new { customerId = result.Value }, result.Value);
        }

        /// <summary>
        /// Removes an existing sales order header.
        /// </summary>
        /// <param name="orderHeaderId">The unique ID of the order header to delete.</param>
        /// <response code="204">The order was successfully removed.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="404">If the order header was not found.</response>
        [HttpDelete("{orderHeaderId}")]
        [Authorize("UserPolicy")]
        public async Task<IActionResult> DeleteOrder(int orderHeaderId)
        {
            var result = await _service.DeleteOrderAsync(orderHeaderId);
            return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage);
        }

        /// <summary>
        /// Performs a full update of an existing order and its associated details.
        /// </summary>
        /// <param name="command">The updated order structure.</param>
        /// <returns>The ID of the updated order.</returns>
        /// <response code="200">Order successfully updated.</response>
        /// <response code="400">If business rules prevent the update.</response>
        /// <response code="401">The user is not authorized.</response>
        [HttpPut]
        [Authorize("UserPolicy")]
        public async Task<IActionResult> UpdateOrder(SalesOrderHeaderCommandDto command)
        {
            var result = await _service.UpdateOrderAsync(command);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }
    }
}