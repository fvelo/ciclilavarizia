using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services
{
    public interface ICustomersService
    {
        /// <summary>
        /// Get all the customers data currently present in the Db
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>IEnumerable of CustomerDto</returns>
        Task<Result<IEnumerable<CustomerDto>>> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a customer present in the db by it's id
        /// </summary>
        /// <param name="id">CustomerId</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return customer if found, otherways null</returns>

        Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a customer in the db if it is present
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the CustomerId of the deleted Customer. If -1 Customer not found</returns>
        Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default);


        // Add other methods: GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync etc.
    }
}
