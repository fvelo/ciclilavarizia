using Ciclilavarizia.Filters;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomersAsync(CancellationToken cancellationToken)
        {
            var result = await _customersService.GetCustomersSummaryAsync(cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpGet("{customerId}")]
        [Authorize("UserPolicy")]
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
            var result = await _customersService.CreateCustomerAsync(customer, cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            return Created("", result.Value);
        }

        [HttpDelete("{id}")]
        [Authorize("UserPolicy")]
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
        [Authorize("UserPolicy")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateCustomerAsync(int id, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken)
        {
            if(id != incomingCustomer.CustomerId) return BadRequest();

            if (!await _customersService.DoesCustomerExistsAsync(id, cancellationToken)) return BadRequest();

            var result = await _customersService.UpdateCustomerByIdAsync(id, incomingCustomer, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpPut("password/{id}")]
        [Authorize("UserPolicy")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateCustomerPasswordAsync(int id, [FromBody] PutPasswordDto newPlainPassword, CancellationToken cancellationToken)
        {
            // in reality here we should send an email to confirm if the real owner is doing this action
            var result = await _customersService.UpdateCustomerPasswordAsync(id, newPlainPassword.PlainPassword);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpPut("email/{id}/{newEmail}")]
        [Authorize("UserPolicy")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateCustomerEmailAsync(int id, string newEmail, CancellationToken cancellationToken)
        {
            // in reality here we should send an email to confirm if the real owner is doing this action
            var result = await _customersService.UpdateCustomerEmailAsync(id, newEmail);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }
    }
}