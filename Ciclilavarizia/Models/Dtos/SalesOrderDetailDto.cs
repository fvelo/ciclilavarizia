namespace Ciclilavarizia.Models.Dtos
{
    // Common Detail Info
    public class SalesOrderDetailDto
    {
        public int SalesOrderDetailId { get; set; }
        public int ProductId { get; set; }
        public short OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        public decimal LineTotal { get; set; }

        public SalesOrderDetailDto(
            int salesOrderDetailId, int productId, short orderQty, decimal unitPrice, decimal unitPriceDiscount, decimal lineTotal)
        {
            SalesOrderDetailId = salesOrderDetailId;
            ProductId = productId;
            OrderQty = orderQty;
            UnitPrice = unitPrice;
            UnitPriceDiscount = unitPriceDiscount;
            LineTotal = lineTotal;
        }
    }
}