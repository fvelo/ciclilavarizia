using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services
{
    public interface IProductsService
    {
        /// <summary>
        /// Get all Products data currently present in the Db
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>IEnumerable of ProductsDto</returns>
        Task<Result<IEnumerable<ProductDto>>> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a Product present in the Db by its id
        /// </summary>
        /// <param name="id">ProductId</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return customer if found, otherways null</returns>
        Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a Product in the db if present
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the ProductId of the deleted Product. If Product not found -1</returns>
        Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Update the Product if present in the Db
        /// </summary>
        /// <param name="id">Id of product</param>
        /// <param name="incomingProduct">Data to change</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns ProductId of the updated Product. If Product NotFound -1.</returns>
        Task<Result<int>> UpdateAsync(int id, ProductDto incomingProduct, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Insert a Product in the Db from the data provided
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns ProductId of the created Product</returns>
        Task<Result<int>> CreateAsync(ProductDto product, CancellationToken cancellationToken = default);
    }
}
