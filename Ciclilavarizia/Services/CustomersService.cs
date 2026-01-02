using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
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

        public async Task<Result<int>> DeleteCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
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

        public async Task<Result<int>> CreateCustomerAsync(Customer incomingCustomer, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

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

        public async Task<bool> DoesCustomerExistsAsync(int customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool exists = await _db.Customers
                    .AsNoTracking()
                    .AnyAsync(c => c.CustomerID == customerId && c.IsDeleted == false, cancellationToken);
                return exists;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("DoesCustomerExistsAsync was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DoesCustomerExistsAsync had a error");
                return false;
            }
        }

        // TODO: finish implementing it
        public async Task<Result<int>> UpdateCustomerByIdAsync(int customerId, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (incomingCustomer == null) return Result<int>.Failure("The customer provided is null.");
                if (incomingCustomer.CustomerId != customerId) return Result<int>.Failure("The id of the customer provided and the id must be the same."); // questa sembra essere una best practrice di controllo

                var existingCustomer = await _db.Customers
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId, cancellationToken);

                if (existingCustomer == null) return Result<int>.Failure("There is not a customer with this Id in the Db.");

                //Console.WriteLine($"Old Customer: {existingCustomer}");

                bool isChanged = false;

                if (!string.IsNullOrEmpty(incomingCustomer.Title) && !string.IsNullOrWhiteSpace(incomingCustomer.Title))
                {
                    existingCustomer.Title = incomingCustomer.Title;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.FirstName) && !string.IsNullOrWhiteSpace(incomingCustomer.FirstName))
                {
                    existingCustomer.FirstName = incomingCustomer.FirstName;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.MiddleName) && !string.IsNullOrWhiteSpace(incomingCustomer.MiddleName))
                {
                    existingCustomer.MiddleName = incomingCustomer.MiddleName;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.LastName) && !string.IsNullOrWhiteSpace(incomingCustomer.LastName))
                {
                    existingCustomer.LastName = incomingCustomer.LastName;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.Suffix) && !string.IsNullOrWhiteSpace(incomingCustomer.Suffix))
                {
                    existingCustomer.Suffix = incomingCustomer.Suffix;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.CompanyName) && !string.IsNullOrWhiteSpace(incomingCustomer.CompanyName))
                {
                    existingCustomer.CompanyName = incomingCustomer.CompanyName;
                    isChanged = true;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.SalesPerson) && !string.IsNullOrWhiteSpace(incomingCustomer.SalesPerson))
                {
                    existingCustomer.SalesPerson = incomingCustomer.SalesPerson;
                    isChanged = true;
                }
                if (isChanged) existingCustomer.ModifiedDate = DateTime.UtcNow;

                // assume _customer already resolved and 'customer' is the incoming DTO
                // handle addresses:
                UpdateCustomerAddresses(existingCustomer, incomingCustomer.CustomerAddresses);

                await _db.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(existingCustomer.CustomerID);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"An error occurred: {ex.Message}");
            }
        }

        private void UpdateCustomerAddresses(Customer existingCustomer, List<CustomerAddressDto> incomingAddressesDto)
        {
            incomingAddressesDto ??= new List<CustomerAddressDto>();

            // addresses in DB that are NOT in the incoming DTO list
            var incomingIds = incomingAddressesDto
                .Where(a => a.AddressId > 0)
                .Select(a => a.AddressId)
                .ToHashSet();

            var addressesToDelete = existingCustomer.CustomerAddresses
                .Where(ca => !incomingIds.Contains(ca.AddressID))
                .ToList();

            foreach (var addressToRemove in addressesToDelete)
            {
                existingCustomer.CustomerAddresses.Remove(addressToRemove);
            }

            foreach (var incomingAddrDto in incomingAddressesDto)
            {
                // Is this an existing address?
                // The new addresses will have 0 as their ID
                var existingAddrEntity = existingCustomer.CustomerAddresses
                    .FirstOrDefault(ca => ca.AddressID == incomingAddrDto.AddressId && incomingAddrDto.AddressId != 0);

                if (existingAddrEntity != null)
                {
                    existingAddrEntity.AddressType = incomingAddrDto.AddressType;

                    if (existingAddrEntity.Address != null && incomingAddrDto.Address != null)
                    {
                        existingAddrEntity.Address.AddressLine1 = incomingAddrDto.Address.AddressLine1;
                        existingAddrEntity.Address.City = incomingAddrDto.Address.City;
                        existingAddrEntity.Address.StateProvince = incomingAddrDto.Address.StateProvince;
                        existingAddrEntity.Address.CountryRegion = incomingAddrDto.Address.CountryRegion;
                        existingAddrEntity.Address.PostalCode = incomingAddrDto.Address.PostalCode;
                        existingAddrEntity.Address.ModifiedDate = DateTime.UtcNow;
                    }
                }
                else
                {
                    // --- ADD NEW ---
                    var newAddressEntity = new CustomerAddress
                    {
                        AddressType = incomingAddrDto.AddressType,
                        ModifiedDate = DateTime.UtcNow,
                        rowguid = Guid.NewGuid(),
                        Address = new Address
                        {
                            AddressLine1 = incomingAddrDto.Address?.AddressLine1 ?? string.Empty,
                            City = incomingAddrDto.Address?.City ?? string.Empty,
                            StateProvince = incomingAddrDto.Address?.StateProvince ?? string.Empty,
                            CountryRegion = incomingAddrDto.Address?.CountryRegion ?? string.Empty,
                            PostalCode = incomingAddrDto.Address?.PostalCode ?? string.Empty,
                            ModifiedDate = DateTime.UtcNow,
                            rowguid = Guid.NewGuid()
                        }
                    };

                    existingCustomer.CustomerAddresses.Add(newAddressEntity);
                }
            }
        }
    }
}
