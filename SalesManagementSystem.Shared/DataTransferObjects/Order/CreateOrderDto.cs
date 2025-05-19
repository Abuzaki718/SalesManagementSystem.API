namespace SalesManagementSystem.Shared.DataTransferObjects.Order;



public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
public sealed class GetOrderDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = null!;
    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }
    public ICollection<GetOrderItemDto>? OrderItems { get; set; }
}
public sealed class GetOrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}