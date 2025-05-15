using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.Core.Entities;
using SalesManagementSystem.Core.Interfaces.IUnitOfWork;
using SalesManagementSystem.Shared.Constants;
using SalesManagementSystem.Shared.DataTransferObjects.Product;
using SalesManagementSystem.Shared.Pagination;
using SalesManagementSystem.Shared.ResponseModles;

namespace SalesManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IUnitOfWork _unitOfWork) : ControllerBase
{



    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<Product>>>> GetProducts()
    {
        var products = await _unitOfWork.ProductRepository.GetAll();
        return Ok(new BaseResponse<List<Product>>(products, "Getting Data Success"));
    }


    /// <summary>
    /// Get All Products with Pagination Using Offset Pagination
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("GetAllWithPagination/{pageNumber}/{pageSize}")]
    public async Task<ActionResult<BaseResponse<PagedResult<Product>>>> GetAllWithPagination([FromRoute] int pageNumber = 1, [FromRoute] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(new BaseResponse<PagedResult<Product>>(null, "Page Number or Page Size is not valid", success: false));


        var products = await _unitOfWork.ProductRepository.GetPagedAsync(x => true, pageNumber, pageSize);
        return Ok(new BaseResponse<PagedResult<Product>>(products, "Getting Data Success"));
    }


    [HttpGet("/{id}")]

    public async Task<ActionResult<BaseResponse<Product>>> GetByd([FromRoute] int id)
    {
        var product = await _unitOfWork.ProductRepository.FindAsync(x => x.ProductId == id);

        if (product is null)
            return NotFound(new BaseResponse<Product>(null, $"The Product With Id {id} Is Not Exist !", success: false));

        return Ok(new BaseResponse<Product>(product, "Getting Data Success"));
    }


    [Authorize(Roles = RolesNames.User)]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<Product>>> Add([FromBody] CreateNewProductDto product)
    {
        if (product is null)
            return BadRequest(new BaseResponse<Product>(null, "Product is null", success: false));

        var addedProduct = await _unitOfWork.ProductRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new BaseResponse<Product>(addedProduct, "Adding Product Success"));
    }


    [Authorize(Roles = RolesNames.User)]
    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<Product>>> Update([FromRoute] int id, [FromBody] UpdateProductDto product)
    {
        if (product is null)
            return BadRequest(new BaseResponse<Product>(null, "Product is null", success: false));

        var productInDb = await _unitOfWork.ProductRepository.FindAsync(x => x.ProductId == id);

        if (productInDb is null)
            return NotFound(new BaseResponse<Product>(null, $"The Product With Id {id} Is Not Exist !", success: false));


        productInDb.ProductName = product.ProductName;
        productInDb.Price = product.Price;
        productInDb.StockQuantity = product.StockQuantity;
        productInDb.Category = product.Category;

        await _unitOfWork.SaveChangesAsync();
        return Ok(new BaseResponse<Product>(productInDb, "Updating Data Success"));
    }

    [Authorize(Roles = RolesNames.User)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse<Product>>> Delete([FromRoute] int id)
    {
        var productInDb = await _unitOfWork.ProductRepository.FindAsync(x => x.ProductId == id);
        if (productInDb is null)
            return NotFound(new BaseResponse<Product>(null, $"The Product With Id {id} Is Not Exist !", success: false));


        if (await _unitOfWork.OrderItemRepository.Any(x => x.ProductId == id))
            return BadRequest(new BaseResponse<Product>(null, "Can't Delete The product si connected With Orders", success: false));


        _unitOfWork.ProductRepository.Delete(productInDb);

        await _unitOfWork.SaveChangesAsync();
        return Ok(new BaseResponse<Product>(productInDb, "Deleting Product Success"));
    }
}
