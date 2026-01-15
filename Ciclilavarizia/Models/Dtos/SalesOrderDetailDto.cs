namespace Ciclilavarizia.Models.Dtos
{
    public class SalesOrderDetailDto
    {
        
        public int SalesOrderId { get; set; }
        public short OrderQty { get; set; } = 1;
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }

        public decimal LineTotal { get; set; }
    }
}
