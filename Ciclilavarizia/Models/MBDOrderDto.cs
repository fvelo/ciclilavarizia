using MongoDB.Bson.Serialization.Attributes;

namespace Ciclilavarizia.Models
{
    public class MBDOrderDto
    {
        public int ClientID { get; set; } = 0;

        [BsonElement("Products")]
        public Dictionary<string, int> Products { get; set; } = new();
    }
}
