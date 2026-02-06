using Ciclilavarizia.Filters;
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

        /// <summary>
        /// Gets all the customers registered the system.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <response code="200">The customers were successfully found.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="500">There was an internal server error processing the request.</response>
        [HttpGet]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetCustomersAsync(CancellationToken cancellationToken)
        {
            var result = await _customersService.GetCustomersSummaryAsync(cancellationToken);

            return Ok(result.Value);
        }

        /// <summary>
        /// Gets the customer details rappresented from customerId.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer to search.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The details of the searched customer.</returns>
        /// <response code="200">The customer was successfully found.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="400">The input data is invalid or fails business rules.</response>
        /// <response code="404">No customer was found with the provided ID.</response>
        /// <response code="500">There was an internal server error processing the request.</response>
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

        /// <summary>
        /// Registers a new customer in the system.
        /// </summary>
        /// <param name="customer">The customer data transfer object containing registration details.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The unique identifier of the newly created customer.</returns>
        /// <response code="201">Returns the ID of the newly created customer.</response>
        /// <response code="400">The input data is invalid or fails business rules.</response>
        /// <response code="500">There was an internal server error processing the request.</response>
        [HttpPost]
        public async Task<ActionResult<int>> PostCustomer(PostCustomerDto customer, CancellationToken cancellationToken)
        {
            var result = await _customersService.CreateCustomerAsync(customer, cancellationToken);

            if (!result.IsSuccess)
                return Problem(detail: result.ErrorMessage);

            return Created("", result.Value);
        }

        /// <summary>
        /// Permanently removes a customer from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The ID of the deleted customer upon success.</returns>
        /// <response code="200">The customer was successfully deleted.</response>
        /// <response code="401">The user is not authorized.</response>
        /// <response code="404">No customer was found with the provided ID.</response>
        /// <response code="500">An error occurred during the deletion process.</response>
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

        /// <summary>
        /// Updates the profile information for an existing customer.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to update (must match the ID in the DTO).</param>
        /// <param name="incomingCustomer">The updated customer detail data.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The ID of the updated customer.</returns>
        /// <response code="200">The profile was updated successfully.</response>
        /// <response code="400">IDs do not match or the update failed validation.</response>
        /// <response code="401">The user is not authorized.</response>
        [HttpPut("{id}")]
        [Authorize("UserPolicy")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateCustomerAsync(int id, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken)
        {
            if (id != incomingCustomer.CustomerId) return BadRequest();

            if (!await _customersService.DoesCustomerExistsAsync(id, cancellationToken)) return BadRequest();

            var result = await _customersService.UpdateCustomerByIdAsync(id, incomingCustomer, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates the password for a specific customer account.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <param name="newPlainPassword">The new password in plain text to be hashed and stored.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The ID of the customer whose password was updated.</returns>
        /// <response code="200">The password has been reset successfully.</response>
        /// <response code="400">The password does not meet security requirements or update failed.</response>
        /// <response code="401">The user is not authorized.</response>
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

        /// <summary>
        /// Changes the primary email address of a customer.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <param name="newEmail">The new valid email address to associate with the account.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The ID of the customer whose email was updated.</returns>
        /// <response code="200">The email address was updated successfully.</response>
        /// <response code="400">The email is already in use or is incorrectly formatted.</response>
        /// <response code="401">The user is not authorized.</response>
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