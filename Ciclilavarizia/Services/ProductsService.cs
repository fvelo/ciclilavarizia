using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Ciclilavarizia.Services
{
    public class ProductsService : IProductsService
    {
        private readonly CiclilavariziaDevContext _context;
        public ProductsService(CiclilavariziaDevContext ctx) 
        {
            _context = ctx;
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


        public async Task<Result<IEnumerable<ProductDto>>> GetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await GetAllProducts().ToListAsync(cancellationToken);
                return Result<IEnumerable<ProductDto>>.Success(products);
            }
            catch (Exception ex) {
                return Result<IEnumerable<ProductDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var ids = await GetAllProducts()
                    .Where(p => p.ProductId == id)
                    .SingleOrDefaultAsync(cancellationToken);

                return Result<ProductDto>.Success(ids);
            }
            catch (Exception ex) 
            {
                return Result<ProductDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<int>> UpdateAsync(int id, ProductDto incomingProduct, CancellationToken cancellationToken = default)
        {
            try 
            {
                var product = await _context.Products.FindAsync(id, cancellationToken);
                if (product != null)
                {
                    _context.Products.Remove(product);
                }
                return await CreateAsync(incomingProduct, cancellationToken);
                
            } catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }

        public async Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _context.Products.FindAsync(id, cancellationToken);
                if (product == null) return Result<int>.Success(-1);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Result<int>.Success(id);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }

        public async Task<Result<int>> CreateAsync(ProductDto product, CancellationToken cancellationToken = default)
        {
            try
            {
                var p = await _context.Products.AddAsync(new Product
                {
                    ProductID = product.ProductId,
                    Name = product.Name,
                    ProductNumber = product.ProductNumber,
                    Color = product.Color,
                    StandardCost = product.StandardCost,
                    ListPrice = product.ListPrice,
                    Size = product.Size,
                    Weight = product.Weight,
                    ProductModel = new ProductModel
                    {
                        Name = product.ProductModel.Name,
                        CatalogDescription = product.ProductModel.CatalogDescription,
                        ProductModelID = product.ProductModel.ProductModelId
                    }
                });

                await _context.SaveChangesAsync(cancellationToken);
                return Result<int>.Success(product.ProductId);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }

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
    }
}
