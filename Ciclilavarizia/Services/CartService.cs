using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Ciclilavarizia.Services
{
    public class CartService
    {
        private readonly IMongoCollection<MdbCart> _context;
        private readonly string _customerId = "CustomerId";

        public CartService(IOptions<MdbConnectionSettings> mdbOptions)
        {
            MongoClient client = new MongoClient(mdbOptions.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mdbOptions.Value.DatabaseName);
            _context = database.GetCollection<MdbCart>(mdbOptions.Value.CollectionName);
        }

        public async Task<Result<List<MdbCartDto>>> GetCartsAsync(CancellationToken cancellationToken = default)
        {
            var carts = await _context.Find(new BsonDocument()).ToListAsync(cancellationToken);
            if (carts == null || carts.Count == 0) return Result<List<MdbCartDto>>.Success(new List<MdbCartDto>()); // it does not matter if there is none, I will send a empty list

            var cartsDto = new List<MdbCartDto>();
            foreach (var cart in carts)
                cartsDto.Add(ConvertCartToCartDto(cart));

            return Result<List<MdbCartDto>>.Success(cartsDto);
        }

        public async Task<Result<MdbCartDto>> GetCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<MdbCart>.Filter.Eq(_customerId, customerId);
            var cart = await _context.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (cart == null) return Result<MdbCartDto>.Failure("Not found!");

            var cartDto = ConvertCartToCartDto(cart);

            return Result<MdbCartDto>.Success(cartDto);
        }

        private MdbCartDto ConvertCartToCartDto(MdbCart cart, CancellationToken cancellationToken = default)
        {
            var cartProducts = new Dictionary<uint, uint>();
            foreach (var product in cart.Products)
                cartProducts.Add(uint.Parse(product.Key), product.Value);

            var cartDto = new MdbCartDto()
            {
                CustomerId = cart.CustomerId,
                Products = cartProducts
            };

            return cartDto;
        }

        public async Task<Result<int>> CreateCartAsync(MdbCartDto cart, CancellationToken cancellationToken = default)
        {
            if (cart == null) return Result<int>.Failure("The cart must contain something.");
            var filter = Builders<MdbCart>.Filter.Eq(_customerId, cart.CustomerId);
            if (await _context.Find(filter).FirstOrDefaultAsync(cancellationToken) != null)
                return Result<int>.Failure("A cart with this Id already exists.");

            var cartProducts = new Dictionary<string, uint>();
            foreach (var product in cart.Products)
            {
                var productId = product.Key.ToString(); //$"{(int)product.Value}"
                var productQty = product.Value;

                if (!uint.TryParse(productId, out uint _)) return Result<int>.Failure("The key of the products must be positive integers.");
                if (productQty == 0) return Result<int>.Failure("The quantity of the products must be at least 1.");
                //if (cartProducts.ContainsKey(productId)) return Result<int>.Failure("Tha cart must contain only one instance per product."); // cart.Products is allready a dictionary it cannot contains two identical keys
                cartProducts.Add(productId, productQty);
            }

            await _context.InsertOneAsync(new MdbCart
            {
                CustomerId = cart.CustomerId,
                Products = cartProducts // serialize as string but make them uint
            });

            return Result<int>.Success(cart.CustomerId);
        }

        public async Task<Result<bool>> DeleteCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<MdbCart>.Filter.Eq(_customerId, customerId);
            var deleted = await _context.DeleteOneAsync(filter, cancellationToken);

            return deleted.DeletedCount > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Not found!");
        }

        public async Task<Result<int>> UpdateCartByCostumerIdAsync(int customerId, MdbCartDto cart, CancellationToken cancellationToken = default)
        {
            if (customerId != cart.CustomerId) return Result<int>.Failure("The customerId is not the same.");
            if (cart == null) return Result<int>.Failure("The cart must contain something.");
            var filter = Builders<MdbCart>.Filter.Eq(_customerId, customerId);
            if (await _context.Find(filter).FirstOrDefaultAsync(cancellationToken) == null)
                return Result<int>.Success(-1); // not found!

            var cartProducts = new Dictionary<string, uint>();
            foreach (var product in cart.Products)
            {
                var productId = product.Key.ToString();
                var productQty = product.Value;

                if (!uint.TryParse(productId, out uint _)) return Result<int>.Failure("The key of the products must be positive integers.");
                if (productQty == 0) return Result<int>.Failure("The quantity of the products must be at least 1.");
                cartProducts.Add(productId, productQty);
            }

            var deleted = await _context.DeleteOneAsync(filter, cancellationToken);
            await _context.InsertOneAsync(new MdbCart
            {
                CustomerId = cart.CustomerId,
                Products = cartProducts
            });

            return Result<int>.Success(cart.CustomerId);
        }
    }
}
