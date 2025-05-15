using SalesManagementSystem.Shared.DataTransferObjects.Product;
using System.Text.Json.Serialization;

namespace SalesManagementSystem.Core.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<OrderItem>? OrderItems { get; set; }

    //Operators For Mapping
    public static implicit operator Product(CreateNewProductDto dto)
    {
        return new Product
        {
            Category = dto.Category,
            ProductName = dto.ProductName,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity
        };
    }
}
