using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Interfaces.IUnitOfWork;
using SalesManagementSystem.Shared.DataTransferObjects.Order;
using SalesManagementSystem.Shared.ResponseModles;
using System.Security.Claims;

namespace SalesManagementSystem.API.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrdersController(IUnitOfWork _unitOfWork) : ControllerBase
{



    [HttpPost]
    public async Task<ActionResult<BaseResponse<Order>>> CreateOrder([FromBody] params CreateOrderItemDto[] orderDto)
    {
        var UserId = HttpContext.User.FindFirstValue("Id");
        if (UserId is null)
            return Unauthorized(new BaseResponse<Order>(null, "The Claim is not found ", success: false)); //Extra safety where the Id is Always with token 

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        var reslut = await _unitOfWork.OrderRepository.CreateOrder(orderDto, UserId);

        if (!reslut.Success)
        {
            await transaction.RollbackAsync();
            return BadRequest(reslut);
        }

        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
        return Ok(reslut);
    }


    [HttpGet]

    public async Task<ActionResult<BaseResponse<List<GetOrderDto>>>> GetAll()
    {

        var result = await _unitOfWork.OrderRepository.GetAllOrders();
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }


    [HttpGet("{id}")]

    public async Task<ActionResult<BaseResponse<GetOrderDto>>> GetById([FromRoute] int id)
    {

        var result = await _unitOfWork.OrderRepository.GetOrderById(id);
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

}
