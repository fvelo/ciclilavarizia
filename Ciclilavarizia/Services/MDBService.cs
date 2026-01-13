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

        public async Task<List<object>> GetAllCarts()
        {

            var carts = await _collection.Find(new BsonDocument()).ToListAsync();
            var mdbJson = new List<object>();
            foreach (var prods in carts)
            {
                mdbJson.Add(prods.ToJson());
            }

            return mdbJson;
        }



        public async Task<List<MDBOrders>> GetOrder(int clientId)
        {

            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task CreateOrder(MDBOrderDto order)
        {

            await _collection.InsertOneAsync(new MDBOrders
            {
                ClientID = order.ClientID,
                Products = order.Products,
            });
            return;
        }


        public async Task AddProducts(int clientId, int product, int quantity)
        {

            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);

            if (quantity > 0) 
            {
                var update = Builders<MDBOrders>.Update.Set($"Products.{product.ToString()}", quantity);
                await _collection.UpdateOneAsync(filter, update);
            }
            else
            {
                var removal = Builders<MDBOrders>.Update.Unset($"Products.{product.ToString()}");
                await _collection.UpdateOneAsync(filter, removal);
            }

            return;
        }

        public async Task DeleteOrder(int clientId)
        {

            var filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            await _collection.DeleteOneAsync(filter);

            return;
        }

    }
}
