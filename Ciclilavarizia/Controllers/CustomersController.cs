using Ciclilavarizia.Data;
using Ciclilavarizia.Filters;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;



namespace Ciclilavarizia.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //for now v1, to change version make a new controller
    public class CustomersController : ControllerBase
    {
        // TODO: Add an actual error logging for Problem() response in every ActionMethod

        private readonly CiclilavariziaDevContext _context;
        private readonly ICustomersService _customersService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(CiclilavariziaDevContext context, ICustomersService customersService, ILogger<CustomersController> logger)
        {
            _context = context;
            _customersService = customersService;
            _logger = logger;
        }

        //[Authorize(Policy = "AdminPolicy")]
        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<CustomerSummaryDto>> GetCustomersAsync(CancellationToken cancellationToken = default)
        {
            var result = await _customersService.GetCustomersSummaryAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning($"GetCustomers failed: {result.ErrorMessage}");
                return Problem(detail: result.ErrorMessage);
            }

            if (result.Value == null)
                return NotFound();

            return Ok(result.Value);
        }

        // GET: api/Customers/5
        [HttpGet("{customerId}")]
        [EnsureCustomerExists(IdParameterName = "customerId")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var result = await _customersService.GetCustomerByIdAsync(customerId, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning($"GetCustomers failed: {result.ErrorMessage}");
                return Problem(detail: result.ErrorMessage);
            }

            if (result.Value is null)
                return NotFound();
            return Ok(result.Value);
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<int>> PostCustomer(PostCustomerDto customer, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _customersService.CreateCustomerAsync(customer, cancellationToken);
                if (!result.IsSuccess)
                {
                    return Problem(result.ErrorMessage);
                }
                //return Ok("Girgio");
                return Ok(result.Value);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"PostCustomer cancelation token");
                return Problem();
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        [EnsureCustomerExists(IdParameterName = "id")]
        public async Task<IActionResult> DeleteCustomer(int id, CancellationToken cancellationToken)
        {
            var result = await _customersService.DeleteCustomerByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning($"DeleteCustomer failed: {result.ErrorMessage}");
                return Problem(result.ErrorMessage);
            }
            if (result.Value == -1)
            {
                return NotFound();
            }

            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [EnsureCustomerExists(IdParameterName ="id")]
        public async Task<IActionResult> UpdateCustomerAsync(int id, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken)
        {
            var result = await _customersService.UpdateCustomerByIdAsync(id, incomingCustomer, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning($"UpdateCustomerAsync failed: {result.ErrorMessage}");
                return Problem(result.ErrorMessage);
            }

            return Ok(result.Value);
        }
    }
}



