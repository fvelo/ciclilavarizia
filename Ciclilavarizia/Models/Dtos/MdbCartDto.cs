using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class MdbCartDto
    {
        [BsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "The CustomerId must be a positive number.")]
        public int CustomerId { get; set; }
        [BsonRequired]
        [BsonElement("Products")]
        public Dictionary<uint, uint> Products { get; set; } = new();

    }
}
