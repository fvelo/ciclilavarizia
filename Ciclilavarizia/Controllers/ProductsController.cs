using Ciclilavarizia.Filters;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;

        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }

        /// <summary>
        /// Retrieves a summary list of all products in the catalog.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>A list of summarized product details.</returns>
        /// <response code="200">Returns the list of product summaries.</response>
        /// <response code="400">If the retrieval operation fails.</response>
        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<List<ProductSummaryDto>>> GetProducts(CancellationToken cancellationToken)
        {
            var result = await _productsService.GetProductsDetailsAsync(cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Retrieves full details for a single product.
        /// </summary>
        /// <param name="id">The unique ID of the product.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The detailed product information.</returns>
        /// <response code="200">Returns the requested product details.</response>
        /// <response code="404">If the product does not exist.</response>
        // GET: api/Products/5
        [HttpGet("{id}")]
        [EnsureProductExists(IdParameterName = "id")]
        public async Task<ActionResult<ProductDetailDto>> GetProductById(int id, CancellationToken cancellationToken)
        {
            var result = await _productsService.GetProductByIdAsync(id, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.ErrorMessage);
        }

        /// <summary>
        /// Adds a new product to the catalog.
        /// </summary>
        /// <param name="productDto">The product information to create.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>The ID of the newly created product.</returns>
        /// <response code="201">Returns the ID of the created product.</response>
        /// <response code="400">If the product data is invalid.</response>
        /// <response code="401">The user is not authorized.</response>
        // POST: api/Products
        [HttpPost]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<int>> CreateProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var result = await _productsService.AddProductAsync(productDto, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Created("", result.Value);
            //return CreatedAtAction(nameof(GetProductById), new { id = result.Value }, result.Value);
        }

        /// <summary>
        /// Updates an existing product's information.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product data.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <response code="204">Product was successfully updated.</response>
        /// <response code="400">If update logic fails or validation errors occur.</response>
        /// <response code="401">The user is not authorized.</response>
        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize("AdminPolicy")]
        [EnsureProductExists(IdParameterName = "id")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto, CancellationToken cancellationToken)
        {
            var result = await _productsService.UpdateProductAsync(id, productDto, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Deletes a product from the catalog.
        /// </summary>
        /// <param name="id">The ID of the product to remove.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <response code="204">Product was successfully deleted.</response>
        /// <response code="400">If deletion is prohibited (e.g., product linked to existing orders).</response>
        /// <response code="401">The user is not authorized.</response>
        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize("AdminPolicy")]
        [EnsureProductExists(IdParameterName = "id")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var result = await _productsService.DeleteProductAsync(id, cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : BadRequest(result.ErrorMessage);
        }
    }
}