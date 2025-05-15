namespace SalesManagementSystem.Core.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem>? OrderItems { get; set; }



}
