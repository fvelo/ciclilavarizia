using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Services
{
    public class ProductsService : IProductsService
    {
        private readonly CiclilavariziaDevContext _context;
        private readonly ILogger<ProductsService> _logger;
        private const string ImageBaseUrl = "/api/images/product/";

        public ProductsService(CiclilavariziaDevContext context, ILogger<ProductsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private IQueryable<Product> AvailableProducts => _context.Products
            .AsNoTracking()
            .Where(p => p.DiscontinuedDate == null
                        && (p.SellEndDate == null || p.SellEndDate > DateTime.UtcNow)
                        && p.SellStartDate <= DateTime.UtcNow);

        public async Task<bool> DoesProductExistsAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AsNoTracking().AnyAsync(p => p.ProductID == productId, cancellationToken);
        }

        public async Task<Result<List<ProductSummaryDto>>> GetProductsDetailsAsync(CancellationToken cancellationToken)
        {
            var items = await AvailableProducts
                .Select(p => new ProductSummaryDto
                {
                    ProductId = p.ProductID,
                    Name = p.Name,
                    ThumbnailUrl = $"{ImageBaseUrl}{p.ProductID}",
                    ProductCategory = p.ProductCategory != null ? p.ProductCategory.Name : "N/A",
                    ProductModel = p.ProductModel != null ? p.ProductModel.Name : "N/A",
                    ListPrice = p.ListPrice,
                    Color = p.Color ?? "N/A"
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductSummaryDto>>.Success(items);
        }

        public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await AvailableProducts
                .Where(p => p.ProductID == id)
                .Select(p => new ProductDetailDto
                {
                    ProductId = p.ProductID,
                    Name = p.Name,
                    ProductNumber = p.ProductNumber,
                    Color = p.Color,
                    ListPrice = (int)Math.Round(p.ListPrice, 0),
                    Size = p.Size ?? "Undefined",
                    Weight = (int?)Math.Round(p.Weight ?? 0, 0) ?? 0,
                    SellStartDate = p.SellStartDate,
                    CatalogDescription = p.ProductModel != null ? p.ProductModel.CatalogDescription : null,
                    Culture = p.ProductModel.ProductModelProductDescriptions
                                 .OrderBy(pm => pm.Culture)
                                 .Select(pm => pm.Culture)
                                 .FirstOrDefault() ?? string.Empty,
                    Description = p.ProductModel.ProductModelProductDescriptions
                                    .OrderBy(pm => pm.Culture)
                                    .Select(pm => pm.ProductDescription.Description)
                                    .FirstOrDefault() ?? string.Empty,
                    ProductCategory = p.ProductCategory != null ? p.ProductCategory.Name : "N/A",
                    ProductModel = p.ProductModel != null ? p.ProductModel.Name : "N/A",
                    ThumbnailUrl = $"{ImageBaseUrl}{p.ProductID}"
                })
                .FirstOrDefaultAsync(cancellationToken);

            return product == null
                ? Result<ProductDetailDto>.Failure("Product not found or unavailable.")
                : Result<ProductDetailDto>.Success(product);
        }

        public async Task<Result<int>> UpdateProductAsync(int productId, ProductDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId, cancellationToken);

            if (entity == null) return Result<int>.Failure("Product not found.");

            entity.Name = dto.Name ?? entity.Name;
            entity.ProductNumber = dto.ProductNumber ?? entity.ProductNumber;
            entity.Color = dto.Color ?? entity.Color;
            entity.StandardCost = dto.StandardCost == null ? entity.StandardCost : dto.StandardCost;
            entity.ListPrice = dto.ListPrice == null ? entity.ListPrice : dto.ListPrice;
            entity.Size = dto.Size ?? entity.Size;
            entity.Weight = dto.Weight ?? entity.Weight;
            entity.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(entity.ProductID);
        }

        public async Task<Result<bool>> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            int rowsAffected = await _context.Products
                .Where(p => p.ProductID == productId)
                .ExecuteDeleteAsync(cancellationToken);

            return rowsAffected > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Product not found.");
        }

        public async Task<Result<int>> AddProductAsync(ProductDto dto, CancellationToken cancellationToken = default)
        {
            var newProduct = new Product
            {
                Name = dto.Name,
                ProductNumber = dto.ProductNumber,
                Color = dto.Color,
                StandardCost = dto.StandardCost == null ? 0 : dto.StandardCost,
                ListPrice = dto.ListPrice == null ? 0 : dto.ListPrice,
                Size = dto.Size,
                Weight = dto.Weight,
                SellStartDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            await _context.Products.AddAsync(newProduct, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(newProduct.ProductID);
        }
    }
}