using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Services.Interfaces
{
    public interface IProductsService
    {
        /// <summary>
        /// Get all Products data currently present in the Db
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of ProductsDto</returns>
        Task<Result<List<ProductSummaryDto>>> GetProductsDetailsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a Product present in the Db by its id
        /// </summary>
        /// <param name="id">ProductId</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return customer if found, otherways null</returns>
        Task<Result<ProductDetailDto>> GetProductByIdAsycn(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a Product in the db if present
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the ProductId of the deleted Product. If Product not found -1</returns>
        //Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the Product if present in the Db
        /// </summary>
        /// <param name="id">Id of product</param>
        /// <param name="incomingProduct">Data to change</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns ProductId of the updated Product. If Product NotFound -1.</returns>
        Task<Result<int>> UpdateProductAsync(int productId, ProductDto product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert a Product in the Db from the data provided
        /// </summary>
        /// <param name="product"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns ProductId of the created Product</returns>
        //Task<Result<int>> CreateAsync(ProductDto product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tells if the product exists in the Db base on it Id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns true if the Product exists, false otherwise</returns>
        Task<bool> DoesProductExistsAsync(int productId, CancellationToken cancellationToken = default);
    }
}
