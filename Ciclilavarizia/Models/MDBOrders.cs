using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Ciclilavarizia.Models
{
    public class MDBOrders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;

        public int ClientID { get; set; } = 0;
        
        [BsonElement("Products")]
        public Dictionary<string,int> Products { get; set; } = new();
    }
}
