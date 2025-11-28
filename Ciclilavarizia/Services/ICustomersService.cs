using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services
{
    public interface ICustomersService
    {
        /// <summary>
        /// Get all the customers data currently present in the Db.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>IEnumerable of CustomerDto.</returns>
        Task<Result<IEnumerable<CustomerDetailDto>>> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a customer present in the db by it's id.
        /// </summary>
        /// <param name="id">CustomerId</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return customer if found, otherways null.</returns>

        Task<Result<CustomerDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a customer in the db if it is present.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the CustomerId of the deleted Customer. If -1 Customer not found.</returns>
        Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a Customer in thr Db based on the data provided.
        /// </summary>
        /// <param name="incomingCustomer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the CustomerId of the Customer created.</returns>
        Task<Result<int>> CreateAsync(Customer incomingCustomer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the Customer in the Db using the data provided.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="incomingCustomer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the CustomerId. If customer not foud -1.</returns>
        Task<Result<int>> UpdateAsync(int id, Customer incomingCustomer, CancellationToken cancellationToken = default);


        /// <summary>
        /// Get all customers for Admin view
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>An IEnumerable of CustomerSummaryDto</returns>
        Task<Result<IEnumerable<CustomerSummaryDto>>> GetCustomersSummaryAsync(CancellationToken cancellationToken = default);

        // Add other methods: GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync etc.
    }
}
