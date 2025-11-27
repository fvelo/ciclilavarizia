using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly CiclilavariziaDevContext _context;
        private readonly ILogger<CustomersService> _logger;

        public CustomersService(CiclilavariziaDevContext context, ILogger<CustomersService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CustomerDto>>> GetAsync(CancellationToken cancellationToken = default)
        {
            try
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
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<CustomerDto>>.Success(customers);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetAsync was cancelled");
                return Result<IEnumerable<CustomerDto>>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting customers");
                return Result<IEnumerable<CustomerDto>>.Failure("An error occurred while retrieving customers.");
            }
        }

        public async Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
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
                .SingleOrDefaultAsync(cancellationToken);

                return Result<CustomerDto>.Success(customer);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetByIdAsync was cancelled");
                return Result<CustomerDto>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting customer");
                return Result<CustomerDto>.Failure("An error occurred while retrieving a customer.");
            }
        }

        public async Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id, cancellationToken);
                if (customer == null)
                {
                    return Result<int>.Success(-1);
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return Result<int>.Success(id);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("DeleteAsync was cancelled");
                return Result<int>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting customer");
                return Result<int>.Failure("An error occurred while deleting a customer.");
            }
        }

        public async Task<Result<int>> CreateAsync(Customer incomingCustomer, CancellationToken cancellationToken = default)
        {
            try
            {
                /*
                NameStyle: 0 di default
                Title: Null
                FisrtName: obbligatorio
                MiddleName: null
                LastName: obbligatorio
                Suffix: null
                CompanyName: null
                SalesPerson: null
                EmailAddress: null, qui, ma diventerà obbligatorio perché serve come autenticaione univoca su Secure
                Phone: null
                PasswordHash: obbligatorio, ma passerà solo su Secure
                PasswordSalt: obbligatorio
                rowguid: obbligatorio, non so come generarlo? boh
                ModifiedDate: obbligatorio, si mette default a now e poi si traduce in stringa
                 */
                Customer customerToAdd = new Customer
                {
                    CustomerID = default, // the Db will assign it
                    NameStyle = false,
                    Title = null,
                    FirstName = incomingCustomer.FirstName,
                    MiddleName = null,
                    LastName = incomingCustomer.LastName,
                    Suffix = null,
                    CompanyName = null,
                    EmailAddress = incomingCustomer.EmailAddress, // può essere null, ma diventerà obbligatoria da aggiungere al Secure. anche se in tanto lo faccio qui
                    Phone = null,
                    PasswordHash = "1234567890", // when the encription is ready implement it
                    PasswordSalt = "1234567890", // when the encription is ready implement it
                    rowguid = default,
                    ModifiedDate = DateTime.Now
                };
                var entity = _context.Customers.Add(customerToAdd);
                _context.SaveChanges();
                var values = entity.CurrentValues;
                var idCreatedCustumer = values.GetValue<int>("CustomerID");
                return Result<int>.Success(idCreatedCustumer);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CreateAsync was cancelled");
                return Result<int>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a customer");
                return Result<int>.Failure("An error occurred while creating a customer.");
            }
        }
        public async Task<Result<int>> UpdateAsync(int id, Customer incomingCustomer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }
}
