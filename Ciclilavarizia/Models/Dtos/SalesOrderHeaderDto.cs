
namespace Ciclilavarizia.Models.Dtos
{
    public class SalesOrderHeaderDto
    {

        // SALES ORDER HEADER
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public byte Status { get; set; }

        public int? OnlineOrderFlag { get; set; }
        public string? PurchaseOrderNumber { get; set; }

        // account number
        public int CustomerID { get; set; }
        // the 2 address are fk, not changable from here
        public string ShipMethod { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }

        public string? Comment { get; set; }

        public List<SalesOrderDetailDto>? SalesOrderDetails { get; set; } = new();
        public int SalesOrderID { get; internal set; }
    }
}
