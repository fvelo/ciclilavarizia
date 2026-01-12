using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services.Interfaces
{
    public interface IProductsService
    {
        /// <summary>
        /// Retrieves a summary list of all available (non-discontinued) products.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of ProductSummaryDto wrapped in a Result.</returns>
        Task<Result<List<ProductSummaryDto>>> GetProductsDetailsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves detailed information for a specific product by its ID, 
        /// verifying availability rules (SellDates and Discontinued status).
        /// </summary>
        /// <param name="id">The unique Product ID.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>ProductDetailDto if found and available; otherwise failure.</returns>
        Task<Result<ProductDetailDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing product using partial data from a DTO.
        /// </summary>
        /// <param name="productId">ID of the product to update.</param>
        /// <param name="product">The DTO containing updated values.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The ID of the updated product or failure.</returns>
        Task<Result<int>> UpdateProductAsync(int productId, ProductDto product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a new product to the database.
        /// </summary>
        /// <param name="product">The product data to insert.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The ID of the newly created product.</returns>
        Task<Result<int>> AddProductAsync(ProductDto product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a product from the database using high-performance bulk deletion.
        /// </summary>
        /// <param name="productId">ID of the product to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if deleted, false if the product was not found.</returns>
        Task<Result<bool>> DeleteProductAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a lightweight check to see if a Product ID exists in the database.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> DoesProductExistsAsync(int productId, CancellationToken cancellationToken = default);
    }
}