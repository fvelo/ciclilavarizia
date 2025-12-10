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


        //[HttpGet("CustomerStream/")]
        //public async IAsyncEnumerable<ActionResult<CustomerDto>> GetCustomersDtoStream(CancellationToken cancellationToken)
        //{
        //    var result = await _customersService.GetAsync(cancellationToken);

        //    if (!result.IsSuccess)
        //    {
        //        _logger.LogWarning($"GetCustomers failed: {result.ErrorMessage}");
        //        yield return Problem(detail: result.ErrorMessage);
        //    }

        //    if (result.Value == null)
        //        yield return NotFound();

        //    List<CustomerDto> customers = result.Value.ToList();
        //    await foreach (var customer in customers)
        //    {
        //        yield return Ok(customer);
        //    }
        //}

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return Ok(CreatedAtAction("GetCustomer", new { id = customer.CustomerID }, customer));

            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id, CancellationToken cancellationToken)
        {
            var result = await _customersService.DeleteAsync(id, cancellationToken);

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

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        //
        // For activity of 29/10/2025
        //

        [HttpGet("listActions/{customerId}")]
        public ActionResult<CustomerDetailDto> GetCustomer(CAndPStore store, int customerId)
        {
            try
            {
                var customer = store._customers.Where(c => c.CustomerId == customerId).Single();
                if (customer == null) { return NotFound(); }
                return Ok(customer);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("listActions/")]
        public ActionResult<List<CustomerDetailDto>> GetCustomersList(CAndPStore store)
        {
            Console.WriteLine("\n\tI entered the GetCustomersList even if there is a Authorize!!!");
            try
            {
                if (store._customers.Count() == 0)
                {
                    var customers = _context.Customers
                    .AsNoTracking()
                    .Select(c => new CustomerDetailDto
                    {
                        CustomerId = c.CustomerID,
                        Title = c.Title,
                        FirstName = c.FirstName,
                        MiddleName = c.MiddleName,
                        LastName = c.LastName,
                        Suffix = c.Suffix,
                        CompanyName = c.CompanyName,
                        SalesPerson = c.SalesPerson,
                        CustomerAddresses = c.CustomerAddresses
                            .Select(ca => new CustomerAddressDto
                            {
                                AddressId = ca.AddressID,
                                AddressType = ca.AddressType,
                                Address = new AddressDto
                                {
                                    AddressLine1 = ca.Address.AddressLine1,
                                    City = ca.Address.City,
                                    StateProvince = ca.Address.StateProvince,
                                    CountryRegion = ca.Address.CountryRegion,
                                    PostalCode = ca.Address.PostalCode
                                }
                            })
                            .ToList()
                    })
                    .ToList();

                    foreach (var custumerFor in customers)
                    {
                        store._customers.Add(custumerFor);
                    }
                }
            }
            catch (Exception)
            {
                return Problem();
            }
            return Ok(store._customers);
        }

        [HttpPost("listActions/")]
        public async Task<ActionResult> AddCustomer([FromBody] CustomerDetailDto customer, CAndPStore store)
        {
            try
            {
                if (store._customers.Count() == 0)
                {
                    var c = await _context.Customers
                        .AsNoTracking()
                        .OrderByDescending(c => c.CustomerID)
                        .Take(1)
                        .SingleOrDefaultAsync();
                    customer.CustomerId = c.CustomerID + 1;
                }
                else
                {
                    customer.CustomerId = store._customers.Last().CustomerId + 1;
                }

                store._customers.Add(customer);
            }
            catch (Exception)
            {
                return Problem();
            }
            return Created();
        }

        [HttpDelete("listActions/{customerId}")]
        public ActionResult DeleteCustomer(int customerId, CAndPStore store)
        {
            try
            {
                var customer = store._customers.FirstOrDefault(c => c.CustomerId == customerId);
                if (customer == null) return NotFound();
                var tmp = store._customers.Remove(customer);
                Console.WriteLine($"Customer deleted: {tmp}");
            }
            catch (Exception)
            {
                return Problem();
            }
            return NoContent();
        }
        // V0
        //[HttpPut("listActions/{customerId}")]
        //public IResult UpdateCustomer(int customerId, [FromBody] CustomerDto customer, CAndPStore store)
        //{
        //    try
        //    {
        //        if (customer.CustomerId != customerId) return Results.BadRequest(); // questa sembra essere una best practrice di controllo

        //        CustomerDto? _customer = store._customers
        //            .Where(c => c.CustomerId == customerId)
        //            .FirstOrDefault();
        //        if (_customer == null) return Results.BadRequest();
        //        if (customer == null) return Results.BadRequest();
        //        Console.WriteLine($"Old Customer: {_customer}");


        //        if (!customer.Title.IsNullOrEmpty())
        //        {
        //            _customer.Title = customer.Title;
        //        }
        //        if (!customer.FirstName.IsNullOrEmpty())
        //        {
        //            _customer.FirstName = customer.FirstName;
        //        }
        //        if (!customer.MiddleName.IsNullOrEmpty())
        //        {
        //            _customer.MiddleName = customer.MiddleName;
        //        }
        //        if (!customer.LastName.IsNullOrEmpty())
        //        {
        //            _customer.LastName = customer.LastName;
        //        }
        //        if (!customer.Suffix.IsNullOrEmpty())
        //        {
        //            _customer.Suffix = customer.Suffix;
        //        }
        //        if (!customer.CompanyName.IsNullOrEmpty())
        //        {
        //            _customer.CompanyName = customer.CompanyName;
        //        }
        //        if (!customer.SalesPerson.IsNullOrEmpty())
        //        {
        //            _customer.SalesPerson = customer.SalesPerson;
        //        }
        //        for (int i = 0; i < _customer.CustomerAddresses.Count(); i++)
        //        {
        //            if (customer.CustomerAddresses.ElementAt(i) == null) continue;
        //            var customerAddress = customer.CustomerAddresses.ElementAt(i);

        //            var _customerAddress = _customer.CustomerAddresses.ElementAt(i);

        //            if (!customerAddress.AddressType.IsNullOrEmpty())
        //            {
        //                _customerAddress.AddressType = customerAddress.AddressType;
        //            }

        //            if (!customerAddress.Address.AddressLine1.IsNullOrEmpty())
        //            {
        //                _customerAddress.Address.AddressLine1 = customerAddress.Address.AddressLine1;
        //            }
        //            if (!customerAddress.Address.City.IsNullOrEmpty())
        //            {
        //                _customerAddress.Address.City = customerAddress.Address.City;
        //            }
        //            if (!customerAddress.Address.StateProvince.IsNullOrEmpty())
        //            {
        //                _customerAddress.Address.StateProvince = customerAddress.Address.StateProvince;
        //            }
        //            if (!customerAddress.Address.CountryRegion.IsNullOrEmpty())
        //            {
        //                _customerAddress.Address.CountryRegion = customerAddress.Address.CountryRegion;
        //            }
        //            if (!customerAddress.Address.PostalCode.IsNullOrEmpty())
        //            {
        //                _customerAddress.Address.PostalCode = customerAddress.Address.PostalCode;
        //            }
        //        }
        //        Console.WriteLine($"New Customer: {_customer}");


        //    }
        //    catch (Exception)
        //    {
        //        return Results.Problem();
        //    }
        //    return Results.NoContent();
        //}
        [HttpPut("listActions/{customerId}")]
        public ActionResult UpdateCustomer(int customerId, [FromBody] CustomerDetailDto customer, CAndPStore store)
        {
            try
            {
                var incomingCustomer = customer; // more readable
                if (incomingCustomer.CustomerId != customerId) return BadRequest(); // questa sembra essere una best practrice di controllo

                CustomerDetailDto? existingCustomer = store._customers
                    .Where(c => c.CustomerId == customerId)
                    .FirstOrDefault();
                if (existingCustomer == null) return BadRequest();
                if (incomingCustomer == null) return BadRequest();
                Console.WriteLine($"Old Customer: {existingCustomer}");


                if (!incomingCustomer.Title.IsNullOrEmpty())
                {
                    existingCustomer.Title = incomingCustomer.Title;
                }
                if (!incomingCustomer.FirstName.IsNullOrEmpty())
                {
                    existingCustomer.FirstName = incomingCustomer.FirstName;
                }
                if (!incomingCustomer.MiddleName.IsNullOrEmpty())
                {
                    existingCustomer.MiddleName = incomingCustomer.MiddleName;
                }
                if (!incomingCustomer.LastName.IsNullOrEmpty())
                {
                    existingCustomer.LastName = incomingCustomer.LastName;
                }
                if (!incomingCustomer.Suffix.IsNullOrEmpty())
                {
                    existingCustomer.Suffix = incomingCustomer.Suffix;
                }
                if (!incomingCustomer.CompanyName.IsNullOrEmpty())
                {
                    existingCustomer.CompanyName = incomingCustomer.CompanyName;
                }
                if (!incomingCustomer.SalesPerson.IsNullOrEmpty())
                {
                    existingCustomer.SalesPerson = incomingCustomer.SalesPerson;
                }

                // assume _customer already resolved and 'customer' is the incoming DTO
                // handle addresses:
                var incomingAddresses = incomingCustomer.CustomerAddresses ?? new List<CustomerAddressDto>();
                var existingAddresses = existingCustomer.CustomerAddresses ?? new List<CustomerAddressDto>();

                // fast lookup with dictionary of existing addresses by id
                var existingById = existingAddresses.ToDictionary(a => a.AddressId);


                if (!incomingAddresses.Any()) // If incoming is empty allora clear all existing addresses
                {
                    existingCustomer.CustomerAddresses.Clear();
                }
                else
                {
                    // Track which existing ids are present in the incoming payload
                    var incomingIds = new HashSet<int>();// this is a Dictionary<Tgaved,bool> do not accept duplicates

                    foreach (var incomingAddr in incomingAddresses)
                    {
                        // if AddressId is present and matches an existing address -> update fields
                        if (incomingAddr.AddressId != 0 && existingById.TryGetValue(incomingAddr.AddressId, out var existAddr)) //make sure to send new addresses with the id set to zero,
                                                                                                                                // this way they will be recognized as new
                        {
                            incomingIds.Add(incomingAddr.AddressId);

                            // update AddressType
                            if (!string.IsNullOrWhiteSpace(incomingAddr.AddressType))
                                existAddr.AddressType = incomingAddr.AddressType;

                            // ensure nested AddressDto exists
                            if (existAddr.Address == null)
                                existAddr.Address = new AddressDto();

                            if (incomingAddr.Address != null)
                            {
                                // update nested fields only when provided
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.AddressLine1))
                                {
                                    existAddr.Address.AddressLine1 = incomingAddr.Address.AddressLine1;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.City))
                                {
                                    existAddr.Address.City = incomingAddr.Address.City;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.StateProvince))
                                {
                                    existAddr.Address.StateProvince = incomingAddr.Address.StateProvince;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.CountryRegion))
                                {
                                    existAddr.Address.CountryRegion = incomingAddr.Address.CountryRegion;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.PostalCode))
                                {
                                    existAddr.Address.PostalCode = incomingAddr.Address.PostalCode;
                                }
                            }
                        }
                        else // address is new
                        {
                            // New address (no id or id not found) -> validate minimal fields then add
                            int newId = (existingAddresses.Any() ? existingAddresses.Max(a => a.AddressId) : 0) + 1; // se esiste un address nel customer then I add 1,
                                                                                                                     // if not 0 WORKS ONLY BECAUSE ARE LIST AND NOT DB

                            var addrToAdd = new CustomerAddressDto
                            {
                                AddressId = newId,
                                AddressType = incomingAddr.AddressType ?? string.Empty,
                                Address = new AddressDto
                                {
                                    AddressLine1 = incomingAddr.Address?.AddressLine1 ?? string.Empty,
                                    City = incomingAddr.Address?.City ?? string.Empty,
                                    StateProvince = incomingAddr.Address?.StateProvince ?? string.Empty,
                                    CountryRegion = incomingAddr.Address?.CountryRegion ?? string.Empty,
                                    PostalCode = incomingAddr.Address?.PostalCode ?? string.Empty
                                }
                            };

                            existingAddresses.Add(addrToAdd); // also updates the existing list reference inside existingCustomer
                            existingById[addrToAdd.AddressId] = addrToAdd;
                            incomingIds.Add(addrToAdd.AddressId);
                        }
                    }

                    // Here I create the actual removing and inserting of the addresses,
                    // more redeable because outside for and it does not impact
                    // Time Complexity, this is still O(n) n= new addresses number
                    var toRemove = existingAddresses.Where(a => !incomingIds.Contains(a.AddressId)).ToList();
                    foreach (var r in toRemove)
                        existingAddresses.Remove(r);

                    // assign back in case existingCustomer.CustomerAddresses was empty
                    existingCustomer.CustomerAddresses = existingAddresses;
                }

                Console.WriteLine($"New Customer: {existingCustomer}");


            }
            catch (Exception)
            {
                return Problem();
            }
            return NoContent();
        }
    }
}



