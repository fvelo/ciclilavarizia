using Ciclilavarizia.Controllers;
using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Threading;

namespace Ciclilavarizia.Services
{
    public class ProductsService : IProductsService
    {
        // TODO: Add an actual error logging for Problem() response in every ActionMethod

        private readonly CiclilavariziaDevContext _context;
        private readonly ILogger<ProductsService> _logger;

        public ProductsService(CiclilavariziaDevContext ctx, ILogger<ProductsService> logger) 
        {
            _context = ctx;
            _logger = logger;
        }
        private IQueryable<ProductDto> GetAllProducts() =>
            _context.Products.AsNoTracking().Include(p => p.ProductModel)
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
                        CatalogDescription = p.ProductModel.CatalogDescription,
                        ProductModelId = p.ProductModel.ProductModelID
                    }
                });

        public async Task<bool> DoesProductExistsAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool exists = await _context.Products
                    .AsNoTracking()
                    .AnyAsync(p => p.ProductID == productId == false, cancellationToken);
                return exists;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Result<List<ProductSummaryDto>>> GetProductsDetailsAsync(CancellationToken cancellationToken)
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
                return Result<List<ProductSummaryDto>>.Success(items);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetProductsDetailsAsync was cancelled");
                return Result<List<ProductSummaryDto>>.Failure("GetProductsDetailsAsync was cancelled");
            }
            catch (Exception)
            {
                return Result<List<ProductSummaryDto>>.Failure("GetProductsDetailsAsync has failed.");
            }
        }

        public async Task<Result<ProductDetailDto>> GetProductByIdAsycn(int id, CancellationToken cancellationToken)
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
                        // I intentionally do not include DiscontinuedDate in the DTO
                        CatalogDescription = p.ProductModel != null ? p.ProductModel.CatalogDescription : null,
                        Culture = p.ProductModel.ProductModelProductDescriptions
                                     .OrderBy(pm => pm.Culture)
                                     .Select(pm => pm.Culture)
                                     .FirstOrDefault() ?? string.Empty,
                        Description = p.ProductModel.ProductModelProductDescriptions
                                        .OrderBy(pm => pm.Culture)
                                        .Select(pm => pm.ProductDescription.Description)
                                        .FirstOrDefault() ?? string.Empty,
                        ProductCategory = p.ProductCategory.Name,
                        ProductModel = p.ProductModel.Name,
                        ThumbnailUrl = "/api/images/product/" + p.ProductID
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // this will be done in the controller

                //if (product == default) // discontinued, selldate ended or sell in future
                //{
                //    return Result<ProductDetailDto>.Success(product);
                //}
                return Result<ProductDetailDto>.Success(product);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetProductByIdAsycn was cancelled");
                return Result<ProductDetailDto>.Failure("GetProductByIdAsycn was cancelled");
            }
            catch (Exception)
            {
                return Result<ProductDetailDto>.Failure("GetProductByIdAsycn had an unexpected problem.");
            }
        }

        public async Task<Result<int>> UpdateProductAsync(int productId, ProductDto product, CancellationToken cancellationToken = default)
        {
            try
            {
                if (product == null) throw new ArgumentNullException();

                var _product = await _context.Products
                    .Where(p => p.ProductID == productId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (_product == default) return Result<int>.Failure("Failure");

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
                return Result<int>.Success(_product.ProductID);

            }
            catch (Exception)
            {
                return Result<int>.Failure("Exeption unhandeld");
            }
        }
    }
}
