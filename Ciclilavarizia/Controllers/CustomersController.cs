using Ciclilavarizia.BLogic;
using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ciclilavarizia.Controllers
{
    [Route("api/v1/[controller]")] //for now v1, to change version make a new controller
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLTContext _context;

        public CustomersController(AdventureWorksLTContext context)
        {
            _context = context;
        }

        //// GET: api/Customers
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        //{
        //    return await _context.Customers.ToListAsync();
        //}

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            List<CustomerDto> customers;
            try
            {
                customers = await _context.Customers
                .AsNoTracking()
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address)
                .Select(c => new CustomerDto
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
                .ToListAsync();
            }
            catch (Exception)
            {
                return Problem();
            }

            return Ok(customers);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            CustomerDto customer;
            try
            {
                customer = await _context.Customers
                .AsNoTracking()
                .Select(c => new CustomerDto
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
                .Where(c => c.CustomerId == id)
                .SingleAsync();

                if (customer == null)
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {
                return Problem();
            }

            return Ok(customer);
        }

        [HttpGet("CustomerStream/")]
        public async IAsyncEnumerable<ActionResult<CustomerDto>> GetCustomersDtoStream()
        {
            var customers = _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
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
            .AsAsyncEnumerable();
            await foreach (var customer in customers)
            {
                yield return Ok(customer);
            }
        }

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
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Problem();
            }

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        //
        // For activity of 29/10/2025
        //

        [HttpGet("listActions/{customerId}")]
        public ActionResult<CustomerDto> GetCustomer(CAndPStore store, int customerId)
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


        [HttpGet("listActions/")]
        public ActionResult<List<CustomerDto>> GetCustomersList(CAndPStore store)
        {
            try
            {
                if (store._customers.Count() == 0)
                {
                    var customers = _context.Customers
                    .AsNoTracking()
                    .Select(c => new CustomerDto
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
        public async Task<ActionResult> AddCustomer([FromBody] CustomerDto customer, CAndPStore store)
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
                if (customer == null) return BadRequest();
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
        public ActionResult UpdateCustomer(int customerId, [FromBody] CustomerDto customer, CAndPStore store)
        {
            try
            {
                var incomingCustomer = customer; // more readable
                if (incomingCustomer.CustomerId != customerId) return BadRequest(); // questa sembra essere una best practrice di controllo

                CustomerDto? existingCustomer = store._customers
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



