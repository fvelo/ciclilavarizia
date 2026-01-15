using MongoDB.Bson.Serialization.Attributes;

namespace Ciclilavarizia.Models.Dtos
{
    public class MDBSingleOrderDto
    {
        public int ClientID { get; set; } = 0;

        
        public int Products { get; set; } = 0;

        public int qty { get; set; } = 0;
    }
}
