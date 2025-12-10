using Ciclilavarizia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class MDBController : ControllerBase
    {
        //the configuration can be done in antoher file
        //and then injected into this class
        private IMongoCollection<MDBOrders> _collection;
        public MDBController(IOptions<MDBConfig> mDBConfig)
        {
            var mongoClient = new MongoClient(mDBConfig.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mDBConfig.Value.DatabaseName);
            _collection = mongoDatabase.GetCollection<MDBOrders>(mDBConfig.Value.CollectionName);
        }

        [HttpGet("/api/Cart/all")]
        public async Task<IActionResult> GetBnBs()
        {
            var bnbs = await _collection.Find(new BsonDocument()).ToListAsync();
            var mdbJson = new List<object>();
            foreach (var item in bnbs)
            {
                mdbJson.Add(item.ToJson());
            }

            return Ok(new { data = mdbJson });
        }

        [HttpGet("/api/Cart/myCart")]
        public async Task<List<MDBOrders>> GetOrders(int clientId)
        {
            FilterDefinition<MDBOrders> filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            return await _collection.Find(filter).ToListAsync();
        }

        [HttpPost("api/Cart/newMyCart")]
        public async Task<IActionResult> CreateOrder([FromBody] MBDOrderDto order)
        {
            try
            {
                await _collection.InsertOneAsync(new MDBOrders
                {
                    ClientID = order.ClientID,
                    Products = order.Products,
                });
                return CreatedAtAction("GetOrders", new { id = order.ClientID, order });
            }
            catch (Exception e) 
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("api/Cart/modifyTot")]
        public async Task<IActionResult> AddProducts(int clientId, int product, int quantity)
        {
            FilterDefinition<MDBOrders> filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);

            if (quantity != 0)
            {
                UpdateDefinition<MDBOrders> update = Builders<MDBOrders>.Update.Set($"Products.{product.ToString()}", quantity);
                await _collection.UpdateOneAsync(filter, update);
            }
            else
            {
                UpdateDefinition<MDBOrders> removal = Builders<MDBOrders>.Update.Unset($"Products.{product.ToString()}");
                await _collection.UpdateOneAsync(filter, removal);
            }
            return NoContent();
        }

        [HttpDelete("api/Cart/deleteCart")]
        public async Task<IActionResult> DeleteOrder(int clientId)
        {
            FilterDefinition<MDBOrders> filter = Builders<MDBOrders>.Filter.Eq("ClientID", clientId);
            await _collection.DeleteOneAsync(filter);
            return NoContent();
        }

    }
}

