using Ciclilavarizia.Filters;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductsService productsService, ILogger<ProductsController> logger)
        {
            _productsService = productsService;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<List<ProductSummaryDto>>> GetProducts(CancellationToken cancellationToken)
        {
            var result = await _productsService.GetProductsDetailsAsync(cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.ErrorMessage);
        }

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