using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers;
[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private IDbService _dbService;
    public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpPost("add-to-warehouse")]
    public async Task<IActionResult> AddProductToWarehouse(Request request){
        //1. validate data
        
        bool productExists = await _dbService.CheckIfProductExists(request.IdProduct);
        if (!productExists)
            return NotFound("Product does not exist.");
        
        bool wareHouseExists  = await _dbService.CheckIfWarehouseExists(request.IdWarehouse);
        if (!wareHouseExists)
            return NotFound("Warehouse does not exist.");

        //2. check if order exists
        int idOrder = await _dbService.GetMatchingOrderId(request.IdProduct, request.Amount, request.CreatedAt);
        if(idOrder == -1)
            return NotFound("Order for product does not exist.");
        
        //3. check if order was realized
        if (await _dbService.CheckIfOrderWasRealized(idOrder))
        {
            return Conflict("Order has already been realized.");
        }
        
        //4. update order
        await _dbService.UpdateOrder(idOrder);
        
        //5. insert to product_warehouse
        int key = await _dbService.InsertProductWarehouse(request.IdProduct,request.IdWarehouse,idOrder,request.Amount);
        return Created(string.Empty, new { id = key });
    }

    [HttpPost("add-to-warehouse-procedural")]
    public async Task<IActionResult> AddProductToWarehouseProcedural(Request request)
    {
        try
        {
            var id = await _dbService.AddProductToWarehouse(request.IdProduct, request.IdWarehouse, request.Amount,
                request.CreatedAt);
            return Created(string.Empty, new { id = id });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
}