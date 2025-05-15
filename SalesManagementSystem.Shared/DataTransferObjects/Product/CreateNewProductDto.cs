namespace SalesManagementSystem.Shared.DataTransferObjects.Product
{
    public sealed class CreateNewProductDto
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
    public sealed class UpdateProductDto
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
