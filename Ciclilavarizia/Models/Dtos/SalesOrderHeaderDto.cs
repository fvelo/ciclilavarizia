namespace Ciclilavarizia.Models.Dtos
{
    // Header View
    public class SalesOrderHeaderDto
    {
        public int SalesOrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public byte Status { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
        public List<SalesOrderDetailDto> SalesOrderDetails { get; set; } = new();

        public SalesOrderHeaderDto(
            int salesOrderId, int customerId, DateTime orderDate, byte status, string purchaseOrderNumber,
            decimal subTotal, decimal taxAmt, decimal freight, decimal totalDue, List<SalesOrderDetailDto> salesOrderDetails)
        {
            SalesOrderId = salesOrderId;
            CustomerId = customerId;
            OrderDate = orderDate;
            Status = status;
            PurchaseOrderNumber = purchaseOrderNumber;
            SubTotal = subTotal;
            TaxAmt = taxAmt;
            Freight = freight;
            TotalDue = totalDue;
            SalesOrderDetails = salesOrderDetails;
        }

    }
}