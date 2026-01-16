using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ciclilavarizia.Services
{
    public class MDBService
    {
        private readonly IMongoCollection<MDBOrders> _collection;

        public MDBService(IOptions<MDBConfig> mDBConfig)
        {
            MongoClient client = new MongoClient(mDBConfig.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mDBConfig.Value.DatabaseName);
            _collection = database.GetCollection<MDBOrders>(mDBConfig.Value.CollectionName);
        }

        public async Task<Result<object>> GetAllOrders()
        {
            var carts = await _collection.Find(new BsonDocument()).ToListAsync();
            var mdbJson = new List<object>();

            foreach (var prods in carts)
            {
                mdbJson.Add(prods.ToJson());
            }

            return mdbJson != null
                ? Result<object>.Success(mdbJson)
                : Result<object>.Success(0);
        }

        public async Task<Result<List<MDBOrders>>> GetOrder(int clientId)
        {
            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            var orders = await _collection.Find(filter).ToListAsync();

            return orders != null && orders.Count > 0
                ? Result<List<MDBOrders>>.Success(orders)
                : Result<List<MDBOrders>>.Success(new List<MDBOrders>());
        }

        public async Task<Result<int>> CreateOrder(MDBOrderDto order)
        {
            if (order == null)
            {
                return Result<int>.Failure("Order data is null");
            }

            foreach (var orderID in order.Products)
            {
                if (!int.TryParse(orderID.Key, out int keyNumber) || keyNumber < 0)
                {
                    return Result<int>.Failure("ProductId is not a positive number.");
                }

                if (orderID.Value < 0)
                {
                    return Result<int>.Failure("ProductId cannot be negative");
                }
            }

            await _collection.InsertOneAsync(new MDBOrders
            {
                ClientID = order.ClientID,
                Products = order.Products,
            });

            return Result<int>.Success(order.ClientID);
        }

        public async Task<Result<int>> AddProducts(int clientId, int product, int quantity)
        {
            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            UpdateDefinition<MDBOrders> update;

            if (quantity > 0)
            {
                update = Builders<MDBOrders>.Update
                    .Set(o => o.ClientID, clientId)        // ensure document exists
                    .Set($"Products.{product}", quantity);
            }
            else if (quantity == 0)
            {
                update = Builders<MDBOrders>.Update
                    .Unset($"Products.{product}");
            }
            else
            {
                return Result<int>.Failure("Quantity cannot be negative");
            }

            var options = new UpdateOptions { IsUpsert = true };  // 🔑 minimal change
            await _collection.UpdateOneAsync(filter, update, options);

            return Result<int>.Success(clientId);
        }

        public async Task<Result<bool>> DeleteOrder(int clientId)
        {
            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            var deleted = await _collection.DeleteOneAsync(filter);

            return deleted.DeletedCount > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Success(false);
        }
    }
}
