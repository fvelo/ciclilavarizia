using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ciclilavarizia.Models
{
    public class MdbCart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null;

        [BsonRequired]
        public int CustomerId { get; set; }

        [BsonRequired]
        [BsonElement("Products")]
        public Dictionary<string, uint> Products { get; set; } = new();
    }
}
