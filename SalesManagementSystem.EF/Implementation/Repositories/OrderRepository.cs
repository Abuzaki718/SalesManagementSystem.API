using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Murur.Core.Domain.Identity;
using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Interfaces.Repositories;
using SalesManagementSystem.EF.DataContext;
using SalesManagementSystem.Shared.DataTransferObjects.Order;
using SalesManagementSystem.Shared.ResponseModles;
using ProductQunity = (int id, int Quntity);
namespace SalesManagementSystem.EF.Implementation.Repositories;

public sealed class OrderRepository : BaseRepository<Order>, IOrderRepository
{


    private readonly UserManager<ApplicationUser> _userManager;

    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<OrderRepository> logger) : base(context)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<BaseResponse<Order>> CreateOrder(CreateOrderItemDto[] orderdto, string userId)
    {
        try
        {
            var User = await _userManager.FindByIdAsync(userId);

            if (User is null)
                return new BaseResponse<Order>(null, "User Not Found", success: false);

            if (!orderdto.Any())
                return new BaseResponse<Order>(null, "Order Items Not Found", success: false);

            List<ProductQunity> products = [];
            foreach (var item in orderdto)
            {


                if (await _context.Products.AnyAsync(product => product.ProductId != item.ProductId || product.Price != item.Price || product.StockQuantity < item.Quantity))
                {
                    return new BaseResponse<Order>(null, $"Product With Id {item.ProductId} Not Found Or Wrong Price or StockQuantity is over ", success: false);

                }

                products.Add((item.ProductId, item.Quantity));


            }

            await _context.Products.Where(p => products.Select(x => x.id).Contains(p.ProductId)).ForEachAsync(p =>
                 {
                     var product = products.FirstOrDefault(x => x.id == p.ProductId);
                     p.StockQuantity -= product.Quntity;

                     if (p.StockQuantity < 5)
                     {
                         _logger.Log(LogLevel.Warning, "Product With Id {ProductId} Stock Quantity is Low", p.ProductId);
                     }
                 });



            await _context.SaveChangesAsync(); //Not A good practice to save changes in repository and this should be don using unit of work  but i will use transaction of unit of work to undo this on failure

            Order order = new Order
            {
                CustomerName = User.FullName,
                OrderDate = DateTime.Now,
                OrderItems = orderdto == null ? null : orderdto.Select(p => new OrderItem
                {

                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    UnitPrice = p.Price,
                }).ToList(),

                TotalAmount = orderdto!.Sum(x => x.Price * x.Quantity),
            };

            await _context.Orders.AddAsync(order);

            return new BaseResponse<Order>(order, "The Order Created Successfully");

        }
        catch (Exception ex)
        {
            return new BaseResponse<Order>(null, "Error Happened", success: false);// This will be logged via Middleware 
        }
    }


    public async Task<BaseResponse<GetOrderDto>> GetOrderById(int id)
    {
        var Order = await _context.Orders.Where(x => x.OrderId == id).Include(p => p.OrderItems).Select(p => new GetOrderDto
        {

            CustomerName = p.CustomerName,
            OrderDate = p.OrderDate,
            OrderId = p.OrderId,
            TotalAmount = p.TotalAmount,
            OrderItems = p.OrderItems.Select(p => new GetOrderItemDto
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice,
            }).ToList()
        }).FirstOrDefaultAsync();

        if (Order is null)
            return new BaseResponse<GetOrderDto>(null, "Order Not Found", success: false);

        return new BaseResponse<GetOrderDto>(Order, "Getting order success");
    }


    public async Task<BaseResponse<List<GetOrderDto>>> GetAllOrders()
    {
        var Orders = await _context.Orders.Include(p => p.OrderItems).Select(p => new GetOrderDto
        {

            CustomerName = p.CustomerName,
            OrderDate = p.OrderDate,
            OrderId = p.OrderId,
            TotalAmount = p.TotalAmount,
            OrderItems = p.OrderItems.Select(p => new GetOrderItemDto
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice,
            }).ToList()
        }).ToListAsync();

        if (!Orders.Any())
            return new BaseResponse<List<GetOrderDto>>(null, "No Orders Not Found", success: false);

        return new BaseResponse<List<GetOrderDto>>(Orders, "Getting orders success");
    }

}
