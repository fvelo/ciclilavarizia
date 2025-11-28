using Ciclilavarizia.Data;
using Ciclilavarizia.Filters;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // TODO: Add an actual error logging for Problem() response in every ActionMethod
        private readonly CiclilavariziaDevContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(CiclilavariziaDevContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ProductSummaryDto>> GetProductsDetailsAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // disconnected logic to better copy/paste :)
                var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.DiscontinuedDate.HasValue);

                var items = await query
                .Select(p => new ProductSummaryDto
                {
                    ProductId = p.ProductID,
                    Name = p.Name,
                    // TODO: build string through a way that will change if the endpoit get modified
                    ThumbnailUrl = "/api/images/product/" + p.ProductID,
                    ProductCategory = p.ProductCategory != null ? p.ProductCategory.Name : "",
                    ProductModel = p.ProductModel != null ? p.ProductModel.Name : "",
                    ListPrice = p.ListPrice,
                    Color = p.Color ?? ""
                })
                .ToListAsync(cancellationToken);
                return Ok(items);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetProductsDetailsAsync was cancelled");
                return BadRequest("GetProductsDetailsAsync was cancelled");
            }
            catch (Exception ex)
            {
                return Problem();
            }
        }

        [EnsureProductExists]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProductByIdAsycn(int id, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;

                cancellationToken.ThrowIfCancellationRequested();

                var product = await _context.Products
                    .AsNoTracking()
                    // Filter by id and availability rules:
                    .Where(p => p.ProductID == id
                                && p.DiscontinuedDate == null // exclude discontinued
                                && (p.SellEndDate == null || p.SellEndDate > now) // exclude sell ended
                                && p.SellStartDate <= now) // Items with future releast date
                    .Select(p => new ProductDetailDto
                    {
                        ProductId = p.ProductID,
                        Name = p.Name,
                        ProductNumber = p.ProductNumber,
                        Color = p.Color,
                        //StandardCost = (int)Math.Round(p.StandardCost, 0),
                        ListPrice = (int)Math.Round(p.ListPrice, 0),
                        Size = p.Size ?? "Undefined",
                        Weight = (int?)Math.Round(p.Weight.Value, 0) ?? 0,
                        SellStartDate = p.SellStartDate,
                        // We intentionally do not include DiscontinuedDate in the projection/DTO output
                        CatalogDescription = p.ProductModel != null ? p.ProductModel.CatalogDescription : null,
                        Culture = p.ProductModel.ProductModelProductDescriptions
                                     .OrderBy(pm => pm.Culture)
                                     .Select(pm => pm.Culture)
                                     .FirstOrDefault() ?? string.Empty,
                        Description = p.ProductModel.ProductModelProductDescriptions
                                        .OrderBy(pm => pm.Culture)
                                        .Select(pm => pm.ProductDescription.Description)
                                        .FirstOrDefault() ?? string.Empty
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (product == null) // discontinued, selldate ended or sell in future
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetProductByIdAsycn was cancelled");
                return BadRequest("GetProductByIdAsycn was cancelled");
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        //// GET: api/Products
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        //{
        //    List<ProductDto> products;
        //    try
        //    {
        //        products = await _context.Products
        //        .Select(p => new ProductDto
        //        {
        //            ProductId = p.ProductID,
        //            Name = p.Name,
        //            ProductNumber = p.ProductNumber,
        //            Color = p.Color,
        //            StandardCost = p.StandardCost,
        //            ListPrice = p.ListPrice,
        //            Size = p.Size,
        //            Weight = p.Weight,
        //            ProductModel = new ProductModelDto
        //            {
        //                Name = p.ProductModel.Name,
        //                CatalogDescription = p.ProductModel.CatalogDescription
        //            }
        //        })
        //        .ToListAsync();
        //    }
        //    catch (Exception)
        //    {
        //        return Problem();
        //    }

        //    return Ok(products);
        //}

        ////GET: api/Products/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<ProductDto>> GetProduct(int id)
        //{
        //    ProductDto product;
        //    try
        //    {
        //        product = await _context.Products
        //        .Select(p => new ProductDto
        //        {
        //            ProductId = p.ProductID,
        //            Name = p.Name,
        //            ProductNumber = p.ProductNumber,
        //            Color = p.Color,
        //            StandardCost = p.StandardCost,
        //            ListPrice = p.ListPrice,
        //            Size = p.Size,
        //            Weight = p.Weight,
        //            ProductModel = new ProductModelDto
        //            {
        //                Name = p.ProductModel.Name,
        //                CatalogDescription = p.ProductModel.CatalogDescription
        //            }
        //        })
        //        .Where(p => p.ProductId == id)
        //        .SingleAsync();

        //        if (product == null)
        //        {
        //            return NotFound();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Problem();
        //    }

        //    return Ok(product);
        //}

        [HttpGet("ProductsStream/")]
        public async IAsyncEnumerable<ActionResult<ProductDto>> GetProductsDtoStream()
        {
            var customers = _context.Products
                .AsNoTracking()
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductID,
                    Name = p.Name,
                    ProductNumber = p.ProductNumber,
                    Color = p.Color,
                    StandardCost = p.StandardCost,
                    ListPrice = p.ListPrice,
                    Size = p.Size,
                    Weight = p.Weight,
                    ProductModel = new ProductModelDto
                    {
                        Name = p.ProductModel.Name,
                        CatalogDescription = p.ProductModel.CatalogDescription
                    }
                })
                .AsAsyncEnumerable();
            await foreach (var customer in customers)
            {
                yield return Ok(customer);
            }
        }


        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductID)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(CreatedAtAction("GetProduct", new { id = product.ProductID }, product));
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return Problem();
            }

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }

        //
        // For activity of 29/10/2025
        //

        [HttpGet("listActions/{productId}")]
        public ActionResult<ProductDto> GetProduct(CAndPStore store, int productId)
        {
            try
            {
                var product = store._products.Where(p => p.ProductId == productId).Single();
                if (product == null) { return NotFound(); }
                return Ok(product);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [HttpGet("listActions/")]
        public ActionResult<List<ProductDto>> GetProductsList(CAndPStore store)
        {
            try
            {
                if (store._products.Count() == 0)
                {
                    var products = _context.Products
                    .Select(p => new ProductDto
                    {
                        ProductId = p.ProductID,
                        Name = p.Name,
                        ProductNumber = p.ProductNumber,
                        Color = p.Color,
                        StandardCost = p.StandardCost,
                        ListPrice = p.ListPrice,
                        Size = p.Size,
                        Weight = p.Weight,
                        ProductModel = new ProductModelDto
                        {
                            Name = p.ProductModel.Name,
                            CatalogDescription = p.ProductModel.CatalogDescription
                        }
                    })
                    .ToList();

                    foreach (var productFor in products)
                    {
                        store._products.Add(productFor);
                    }
                }
            }
            catch (Exception)
            {
                return Problem();
            }

            return Ok(store._products);
        }

        [HttpPost("listActions/")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto product, CAndPStore store)
        {
            if (store._products.Count() == 0)
            {
                var p = await _context.Products
                    .AsNoTracking()
                    .Take(1)
                    .OrderByDescending(p => p.ProductID)
                    .SingleOrDefaultAsync();
                //Console.WriteLine($"guguggagagag: {g.ProductId}");
                product.ProductId = p.ProductID + 1;
            }
            else
            {
                product.ProductId = store._products.Last().ProductId + 1;
            }

            try
            {
                store._products.Add(product);
            }
            catch (Exception)
            {
                return Problem();
            }
            return Created();
        }

        [HttpDelete("listActions/{productId}")]
        public IActionResult DeleteProduct(int productId, CAndPStore store)
        {
            try
            {
                var product = store._products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null) return BadRequest();
                store._products.Remove(product);
            }
            catch (Exception)
            {
                return Problem();
            }
            return NoContent();
        }

        [HttpPut("listActions/{productId}")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductDto product, CAndPStore store)
        {
            try
            {
                ProductDto? _product = store._products
                    .Where(p => p.ProductId == productId)
                    .FirstOrDefault();
                if (_product == null) return BadRequest();
                if (product == null) return BadRequest();

                if (!product.Name.IsNullOrEmpty()) //this is very puzzolente e brutto, but the "foreach" way to do is much harder and confusing, I will keep it for the v2
                {
                    _product.Name = product.Name;
                }
                if (!product.ProductNumber.IsNullOrEmpty())
                {
                    _product.ProductNumber = product.ProductNumber;
                }
                if (!product.Color.IsNullOrEmpty())
                {
                    _product.Color = product.Color;
                }
                if (product.StandardCost != null)
                {
                    _product.StandardCost = product.StandardCost;
                }
                if (product.ListPrice != null)
                {
                    _product.ListPrice = product.ListPrice;
                }
                if (!product.Size.IsNullOrEmpty())
                {
                    _product.Size = product.Size;
                }
                if (product.Weight != null)
                {
                    _product.Weight = product.Weight;
                }
            }
            catch (Exception)
            {
                return Problem();
            }
            return NoContent();
        }
    }
}
