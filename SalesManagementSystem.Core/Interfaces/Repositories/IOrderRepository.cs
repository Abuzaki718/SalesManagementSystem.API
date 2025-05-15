using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Settings;
using SalesManagementSystem.Shared.DataTransferObjects.Order;
using SalesManagementSystem.Shared.ResponseModles;

namespace SalesManagementSystem.Core.Interfaces.Repositories;

public interface IOrderRepository : IBaseRepository<Order>, IScopedInterface
{

    Task<BaseResponse<Order>> CreateOrder(CreateOrderDto order, string userId);
    Task<BaseResponse<GetOrderDto>> GetOrderById(int id);

    Task<BaseResponse<List<GetOrderDto>>> GetAllOrders();
}
