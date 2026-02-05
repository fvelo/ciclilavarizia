using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using CommonCiclilavarizia;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly CiclilavariziaDevContext _db;
        private readonly SecureDbService _secureDb;
        private readonly Encryption _encryptionHandler;

        public CustomersService(CiclilavariziaDevContext db, SecureDbService secureDb, Encryption encryptionHandler)
        {
            _db = db;
            _secureDb = secureDb;
            _encryptionHandler = encryptionHandler;
        }

        public async Task<Result<IEnumerable<CustomerSummaryDto>>> GetCustomersSummaryAsync(CancellationToken cancellationToken = default)
        {
            var customers = await _db.Customers
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Select(c => new CustomerSummaryDto
                {
                    CustomerId = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToListAsync(cancellationToken);

            if (!customers.Any())
            {
                return Result<IEnumerable<CustomerSummaryDto>>.Success(Enumerable.Empty<CustomerSummaryDto>());
            }

            var customerSummaryTasks = customers.Select(async c =>
            {
                c.EmailAddress = await _secureDb.GetEmailAddressByCustomerIdAsync(c.CustomerId);
                return c;
            });

            var customerSummaries = await Task.WhenAll(customerSummaryTasks);
            return Result<IEnumerable<CustomerSummaryDto>>.Success(customerSummaries);
        }

        public async Task<Result<IEnumerable<CustomerDetailDto>>> GetCustomersAsync(CancellationToken cancellationToken = default)
        {
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
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<CustomerDetailDto>>.Success(customers);
        }

        public async Task<Result<CustomerDetailDto>> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _db.Customers
                .AsNoTracking()
                .Where(c => c.CustomerID == id && !c.IsDeleted)
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
                        }).ToList()
                })
                .SingleOrDefaultAsync(cancellationToken);

            return customer != null
                ? Result<CustomerDetailDto>.Success(customer)
                : Result<CustomerDetailDto>.Failure("Customer not found.");
        }

        //public async Task<Result<int>> DeleteCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
        //{
        //    var customer = await _db.Customers.FindAsync(new object[] { id }, cancellationToken);

        //    if (customer == null) return Result<int>.Success(-1);

        //    //var existingCustomer = await _db.Customers
        //    //    .FirstOrDefaultAsync(c => c.CustomerID == id, cancellationToken);

        //    //if (existingCustomer == null) return Result<int>.Failure("Customer not found.");

        //    //existingCustomer.IsDeleted = true;


        //    _db.Customers.Remove(customer);
        //    await _db.SaveChangesAsync(cancellationToken);
        //    return Result<int>.Success(id);
        //}

        public async Task<Result<int>> DeleteCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // We include relations to ensure delete in the db
            var customer = await _db.Customers
                .Include(c => c.CustomerAddresses)
                .Include(c => c.SalesOrderHeaders)
                .SingleOrDefaultAsync(c => c.CustomerID == id, cancellationToken);

            if (customer == null)
            {
                return Result<int>.Success(-1);
            }

            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            // This prevents Foreign Key constraints from blocking the delete
            if (customer.CustomerAddresses != null && customer.CustomerAddresses.Any()) // you do need to implement every possibile connection with the fk in the db
            {
                _db.CustomerAddresses.RemoveRange(customer.CustomerAddresses);
            }
            if (customer.SalesOrderHeaders != null && customer.SalesOrderHeaders.Any())
            {
                _db.SalesOrderHeaders.RemoveRange(customer.SalesOrderHeaders);
            }

            _db.Customers.Remove(customer);

            await _db.SaveChangesAsync(cancellationToken);

            bool secureDbDeleted = await _secureDb.DeleteCredentialByCustomerIdAsync(id);

            if (!secureDbDeleted)
            {
                return Result<int>.Failure("Failed to delete security credentials. Operation rolled back.");
            }

            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(id);
        }

        public async Task<Result<int>> CreateCustomerAsync(PostCustomerDto incomingCustomer, CancellationToken cancellationToken = default)
        {
            if (await _secureDb.DoesCredentialExistsByEmailAsync(incomingCustomer.EmailAddress))
            {
                return Result<int>.Failure("The email address is already registered.");
            }

            // The problem here is that we work with two db, the main and the secure. The secure is managed through ado.net
            // this makes it more "secure" and less abstact but it is littearally another db to manage and there is no
            // comunication between the two. If the process of creating the customer for some reason have problems after creatin the 
            // Customer in the main db, this does not have any Credentials and this is a big problem. For this reason I will use a transaction


            // Start a Transaction on the Main DB
            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            Customer customerToAdd = new Customer
            {
                NameStyle = false,
                FirstName = incomingCustomer.FirstName,
                LastName = incomingCustomer.LastName,
                rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.Now
            };

            await _db.Customers.AddAsync(customerToAdd, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken); // Customer gets an ID here, but is NOT fully committed yet

            var salt = _encryptionHandler.GenerateSalt();
            Credentials credentials = new Credentials
            {
                CustomerId = customerToAdd.CustomerID,
                EmailAddress = incomingCustomer.EmailAddress,
                PasswordHash = _encryptionHandler.HashPassword(incomingCustomer.PlainPassword, salt),
                PasswordSalt = salt,
                Role = /* incomingCustomer.Role ?? */ "User"
            };

            // Attempt to save to Secure DB
            var createdCredentialId = await _secureDb.CreateCredentialAsync(credentials);

            if (createdCredentialId == -1)
            {
                return Result<int>.Failure("Account security setup failed. Process rolled back.");
            }

            // If it reached here, both DB operations are ready.
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(customerToAdd.CustomerID);
        }

        public async Task<bool> DoesCustomerExistsAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _db.Customers
                .AsNoTracking()
                .AnyAsync(c => c.CustomerID == customerId && !c.IsDeleted, cancellationToken);
        }

        public async Task<Result<int>> UpdateCustomerByIdAsync(int customerId, CustomerDetailDto incomingCustomer, CancellationToken cancellationToken = default)
        {
            if (incomingCustomer == null) return Result<int>.Failure("The customer provided is null.");
            if (incomingCustomer.CustomerId != customerId) return Result<int>.Failure("ID mismatch.");

            var existingCustomer = await _db.Customers
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId, cancellationToken);

            if (existingCustomer == null) return Result<int>.Failure("Customer not found.");

            existingCustomer.Title = incomingCustomer.Title ?? existingCustomer.Title;
            existingCustomer.FirstName = incomingCustomer.FirstName ?? existingCustomer.FirstName;
            existingCustomer.MiddleName = incomingCustomer.MiddleName ?? existingCustomer.MiddleName;
            existingCustomer.LastName = incomingCustomer.LastName ?? existingCustomer.LastName;
            existingCustomer.Suffix = incomingCustomer.Suffix ?? existingCustomer.Suffix;
            existingCustomer.CompanyName = incomingCustomer.CompanyName ?? existingCustomer.CompanyName;
            existingCustomer.SalesPerson = incomingCustomer.SalesPerson ?? existingCustomer.SalesPerson;
            existingCustomer.ModifiedDate = DateTime.UtcNow;

            UpdateCustomerAddresses(existingCustomer, incomingCustomer.CustomerAddresses);

            await _db.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(existingCustomer.CustomerID);
        }

        private void UpdateCustomerAddresses(Customer existingCustomer, List<CustomerAddressDto> incomingAddressesDto)
        {
            incomingAddressesDto ??= new List<CustomerAddressDto>();

            var incomingIds = incomingAddressesDto
                .Where(a => a.AddressId > 0)
                .Select(a => a.AddressId)
                .ToHashSet();

            var addressesToDelete = existingCustomer.CustomerAddresses
                .Where(ca => !incomingIds.Contains(ca.AddressID))
                .ToList();

            foreach (var addressToRemove in addressesToDelete)
                existingCustomer.CustomerAddresses.Remove(addressToRemove);

            foreach (var incomingAddrDto in incomingAddressesDto)
            {
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
                    existingCustomer.CustomerAddresses.Add(new CustomerAddress
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
                    });
                }
            }
        }

        public async Task<Result<bool>> UpdateCustomerPasswordAsync(int customerId, string newPlainPassword)
        {
            if (string.IsNullOrWhiteSpace(newPlainPassword))
                return Result<bool>.Failure("Password cannot be empty.");

            var newSalt = _encryptionHandler.GenerateSalt();
            var newHash = _encryptionHandler.HashPassword(newPlainPassword, newSalt);

            bool isUpdated = await _secureDb.UpdatePasswordAsync(customerId, newHash, newSalt);

            if (!isUpdated)
            {
                return Result<bool>.Failure("Failed to update password. Customer not found in Secure DB.");
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateCustomerEmailAsync(int customerId, string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                return Result<bool>.Failure("Email cannot be empty.");

            string sanitizedEmail = newEmail.Trim().ToLower().Replace(" ", "");

            int? existingOwnerId = await _secureDb.GetCustomerIdByEmailAddressAsync(sanitizedEmail);

            if (existingOwnerId != null && existingOwnerId != customerId)
            {
                return Result<bool>.Failure("This email address is already in use by another account.");
            }

            bool isUpdated = await _secureDb.UpdateEmailAsync(customerId, sanitizedEmail);

            if (!isUpdated)
            {
                return Result<bool>.Failure("Failed to update email. Customer not found in Secure DB.");
            }

            return Result<bool>.Success(true);
        }
    }
}