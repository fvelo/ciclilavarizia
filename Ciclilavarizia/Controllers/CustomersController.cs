using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ciclilavarizia.BLogic;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Data;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
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
            var customers = await _context.Customers
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
            return customers;
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers
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

            return customer;
        }

        [HttpGet("CustomerStream/")]
        public async IAsyncEnumerable<CustomerDto> GetCustomersDtoStream()
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
                yield return customer;
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
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.CustomerID }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

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
        public CustomerDto GetCustomer(CAndPStore store, int customerId)
        {
            return store._customers.Where(c => c.CustomerId == customerId).Single();
        }


        [HttpGet("listActions/")]
        public List<CustomerDto> GetCustomersList(CAndPStore store)
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
            return store._customers;
        }

        [HttpPost("listActions/")]
        public async Task<IResult> AddCustomer([FromBody] CustomerDto customer, CAndPStore store)
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

            try
            {
                store._customers.Add(customer);
            }
            catch (Exception)
            {
                return Results.Problem();
            }
            return Results.Created();
        }

        [HttpDelete("listActions/{customerId}")]
        public IResult DeleteCustomer(int customerId, CAndPStore store)
        {
            try
            {
                var customer = store._customers.FirstOrDefault(c => c.CustomerId == customerId);
                if (customer == null) return Results.BadRequest();
                store._customers.Remove(customer);
            }
            catch (Exception)
            {
                return Results.Problem();
            }
            return Results.NoContent();
        }

        [HttpPut("listActions/{customerId}")]
        public IResult UpdateCustomer(int customerId, [FromBody] CustomerDto customer, CAndPStore store)
        {
            try
            {
                CustomerDto? _customer = store._customers
                    .Where(c => c.CustomerId == customerId)
                    .FirstOrDefault();
                if (_customer == null) return Results.BadRequest();
                if (customer == null) return Results.BadRequest();

                if (!customer.Title.IsNullOrEmpty())
                {
                    _customer.Title = customer.Title;
                }
                if (!customer.FirstName.IsNullOrEmpty())
                {
                    _customer.FirstName = customer.FirstName;
                }
                if (!customer.MiddleName.IsNullOrEmpty())
                {
                    _customer.MiddleName = customer.MiddleName;
                }
                if (!customer.Suffix.IsNullOrEmpty())
                {
                    _customer.Suffix = customer.Suffix;
                }
                if (!customer.CompanyName.IsNullOrEmpty())
                {
                    _customer.CompanyName = customer.CompanyName;
                }
                if (!customer.SalesPerson.IsNullOrEmpty())
                {
                    _customer.SalesPerson = customer.SalesPerson;
                }
                for (int i = 0; i < _customer.CustomerAddresses.Count(); i++)
                {
                    if (customer.CustomerAddresses.ElementAt(i) == null) continue;
                    var customerAddress = customer.CustomerAddresses.ElementAt(i);

                    var _customerAddress = _customer.CustomerAddresses.ElementAt(i);

                    if (!customerAddress.AddressType.IsNullOrEmpty())
                    {
                        _customerAddress.AddressType = customerAddress.AddressType;
                    }

                    if (!customerAddress.Address.AddressLine1.IsNullOrEmpty())
                    {
                        _customerAddress.Address.AddressLine1 = customerAddress.Address.AddressLine1;
                    }
                    if (!customerAddress.Address.City.IsNullOrEmpty())
                    {
                        _customerAddress.Address.City = customerAddress.Address.City;
                    }
                    if (!customerAddress.Address.StateProvince.IsNullOrEmpty())
                    {
                        _customerAddress.Address.StateProvince = customerAddress.Address.StateProvince;
                    }
                    if (!customerAddress.Address.CountryRegion.IsNullOrEmpty())
                    {
                        _customerAddress.Address.CountryRegion = customerAddress.Address.CountryRegion;
                    }
                    if (!customerAddress.Address.PostalCode.IsNullOrEmpty())
                    {
                        _customerAddress.Address.PostalCode = customerAddress.Address.PostalCode;
                    }
                }
            }
            catch (Exception)
            {
                return Results.Problem();
            }
            return Results.NoContent();
        }
    }
}
