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
        public async Task<Result<int>> UpdateAsync(int customerId, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken = default)
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


                if (!string.IsNullOrEmpty(incomingCustomer.Title) && !string.IsNullOrWhiteSpace(incomingCustomer.Title))
                {
                    existingCustomer.Title = incomingCustomer.Title;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.FirstName) && !string.IsNullOrWhiteSpace(incomingCustomer.FirstName))
                {
                    existingCustomer.FirstName = incomingCustomer.FirstName;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.MiddleName) && !string.IsNullOrWhiteSpace(incomingCustomer.MiddleName))
                {
                    existingCustomer.MiddleName = incomingCustomer.MiddleName;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.LastName) && !string.IsNullOrWhiteSpace(incomingCustomer.LastName))
                {
                    existingCustomer.LastName = incomingCustomer.LastName;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.Suffix) && !string.IsNullOrWhiteSpace(incomingCustomer.Suffix))
                {
                    existingCustomer.Suffix = incomingCustomer.Suffix;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.CompanyName) && !string.IsNullOrWhiteSpace(incomingCustomer.CompanyName))
                {
                    existingCustomer.CompanyName = incomingCustomer.CompanyName;
                }
                if (!string.IsNullOrEmpty(incomingCustomer.SalesPerson) && !string.IsNullOrWhiteSpace(incomingCustomer.SalesPerson))
                {
                    existingCustomer.SalesPerson = incomingCustomer.SalesPerson;
                }

                // assume _customer already resolved and 'customer' is the incoming DTO
                // handle addresses:
                var incomingAddresses = incomingCustomer.CustomerAddresses ?? new List<CustomerAddressDto>();
                var existingAddresses = existingCustomer.CustomerAddresses ?? new List<CustomerAddress>();

                // fast lookup with dictionary of existing addresses by id
                var existingById = existingAddresses.ToDictionary(a => a.AddressID);


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
                                existAddr.Address = new Address();

                            if (incomingAddr.Address != null)
                            {
                                // update nested fields only when provided
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.AddressLine1) && !string.IsNullOrEmpty(incomingAddr.Address.AddressLine1))
                                {
                                    existAddr.Address.AddressLine1 = incomingAddr.Address.AddressLine1;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.City) && !string.IsNullOrEmpty(incomingAddr.Address.City))
                                {
                                    existAddr.Address.City = incomingAddr.Address.City;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.StateProvince) && !string.IsNullOrEmpty(incomingAddr.Address.StateProvince))
                                {
                                    existAddr.Address.StateProvince = incomingAddr.Address.StateProvince;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.CountryRegion) && !string.IsNullOrEmpty(incomingAddr.Address.CountryRegion))
                                {
                                    existAddr.Address.CountryRegion = incomingAddr.Address.CountryRegion;
                                }
                                if (!string.IsNullOrWhiteSpace(incomingAddr.Address.PostalCode) && !string.IsNullOrEmpty(incomingAddr.Address.PostalCode))
                                {
                                    existAddr.Address.PostalCode = incomingAddr.Address.PostalCode;
                                }
                            }
                        }
                        else // address is new
                        {
                            // New address (no id or id not found) -> validate minimal fields then add
                            int newId = (existingAddresses.Any() ? existingAddresses.Max(a => a.AddressID) : 0) + 1; // se esiste un address nel customer then I add 1,
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

                var entity = _db.Customers.ExecuteUpdateAsync(existingCustomer, cancellationToken);
                await _db.SaveChangesAsync();
                var values = entity.Result;
                var idCreatedCustumer = values.GetValue<int>("CustomerID");
                return Result<int>.Success(idCreatedCustumer);

                //Console.WriteLine($"New Customer: {existingCustomer}");
                //return Result<int>.Failure("");
            }
            catch (Exception)
            {
                return Result<int>.Failure("");
            }
        }
    }
}
