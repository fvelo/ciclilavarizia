using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly AdventureWorksLTContext _context;
        private readonly ILogger<CustomersService> _logger;

        public CustomersService(AdventureWorksLTContext context, ILogger<CustomersService> logger)
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
    }
}
