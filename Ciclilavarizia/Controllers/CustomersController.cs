using Ciclilavarizia.Filters;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersService _customersService;

        public CustomersController(ICustomersService customersService)
        {
            _customersService = customersService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomersAsync(CancellationToken cancellationToken)
        {
            var result = await _customersService.GetCustomersSummaryAsync(cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpGet("{customerId}")]
        [EnsureCustomerExists(IdParameterName = "customerId")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerAsync(int customerId, CancellationToken cancellationToken)
        {
            var result = await _customersService.GetCustomerByIdAsync(customerId, cancellationToken);

            if (result.Value is null)
                return NotFound();

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<int>> PostCustomer(PostCustomerDto customer, CancellationToken cancellationToken)
        {
            if(await _customersService.DoesCustomerExistsAsync(customer.CustomerId, cancellationToken)) return BadRequest();

            var result = await _customersService.CreateCustomerAsync(customer, cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            // Best practice: return 201, quindi accrocchio
            return CreatedAtAction(nameof(GetCustomerAsync), new { customerId = result.Value }, result.Value);
        }

        [HttpDelete("{id}")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> DeleteCustomer(int id, CancellationToken cancellationToken)
        {
            var result = await _customersService.DeleteCustomerByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            if (result.Value == -1)
                return NotFound();

            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateCustomerAsync(int id, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken)
        {
            if(id != incomingCustomer.CustomerId) return BadRequest();

            if (await _customersService.DoesCustomerExistsAsync(id, cancellationToken)) return BadRequest();

            var result = await _customersService.UpdateCustomerByIdAsync(id, incomingCustomer, cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            return Ok(result.Value);
        }
    }
}