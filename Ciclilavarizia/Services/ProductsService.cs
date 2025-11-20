using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services
{
    public class ProductsService: IProductsService
    {
        public async Task<Result<IEnumerable<ProductDto>>> GetAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<Result<IEnumerable<ProductDto>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<Result<int>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<Result<int>> UpdateAsync(int id, ProductDto incomingProduct, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<Result<int>> CreateAsync(ProductDto product, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
