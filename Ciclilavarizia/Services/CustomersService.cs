using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class CustomersService : ICustomersService
    {
        // TODO: Create an actual error handling

        private readonly CiclilavariziaDevContext _db;
        private readonly ILogger<CustomersService> _logger;
        private readonly SecureDbService _secureDb;


        public CustomersService(CiclilavariziaDevContext db, ILogger<CustomersService> logger, SecureDbService secureDb)
        {
            _db = db;
            _logger = logger;
            _secureDb = secureDb;
        }

        public async Task<Result<IEnumerable<CustomerSummaryDto>>> GetCustomersSummaryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customers = await _db.Customers
                .AsNoTracking()
                .Where(c => c.IsDeleted == false)
                .Select(c => new CustomerSummaryDto
                {
                    CustomerId = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToListAsync(cancellationToken);

                if (!customers.Any())
                {
                    return Result<IEnumerable<CustomerSummaryDto>>
                        .Success(Enumerable.Empty<CustomerSummaryDto>());
                }

                var customerSummaryTasks = customers.Select(async c =>
                {
                    string emailAddress = await _secureDb.GetEmailAddressByCustomerIdAsync(c.CustomerId);

                    return new CustomerSummaryDto
                    {
                        CustomerId = c.CustomerId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        EmailAddress = emailAddress
                    };
                }).ToList();

                var customerSummaries = await Task.WhenAll(customerSummaryTasks); // Execute the list of Tasks passed

                return Result<IEnumerable<CustomerSummaryDto>>.Success(customerSummaries);
            }
            catch (OperationCanceledException)
            {
                return Result<IEnumerable<CustomerSummaryDto>>.Failure($"Request Cancelled.");
            }
            catch (Exception)
            {
                return Result<IEnumerable<CustomerSummaryDto>>.Failure($"Unexpected error.");
                throw;
            }
        }

        public async Task<Result<IEnumerable<CustomerDetailDto>>> GetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customers = await _db.Customers
                    .AsNoTracking()
                    .Include(c => c.CustomerAddresses)
                        .ThenInclude(ca => ca.Address)
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
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<CustomerDetailDto>>.Success(customers);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetAsync was cancelled");
                return Result<IEnumerable<CustomerDetailDto>>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting customers");
                return Result<IEnumerable<CustomerDetailDto>>.Failure("An error occurred while retrieving customers.");
            }
        }

        public async Task<Result<CustomerDetailDto>> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customer = await _db.Customers
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
                .Where(c => c.CustomerId == id)
                .SingleOrDefaultAsync(cancellationToken);

                return Result<CustomerDetailDto>.Success(customer);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetByIdAsync was cancelled");
                return Result<CustomerDetailDto>.Failure("Operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting customer");
                return Result<CustomerDetailDto>.Failure("An error occurred while retrieving a customer.");
            }
        }

        public async Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customer = await _db.Customers.FindAsync(id, cancellationToken);
                if (customer == null)
                {
                    return Result<int>.Success(-1);
                }

                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
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
                    //EmailAddress = incomingCustomer.EmailAddress, // può essere null, ma diventerà obbligatoria da aggiungere al Secure. anche se in tanto lo faccio qui
                    Phone = null,
                    //PasswordHash = "1234567890", // when the encription is ready implement it
                    //PasswordSalt = "1234567890", // when the encription is ready implement it
                    rowguid = default,
                    ModifiedDate = DateTime.Now
                };
                var entity = _db.Customers.Add(customerToAdd);
                _db.SaveChanges();
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
