namespace Models.Models.Dtos
{
    public class ProductModelDto
    {
        public int ProductModelId { get; set; }
        public string Name { get; set; } = null!;
        public string? CatalogDescription { get; set; }
    }
}