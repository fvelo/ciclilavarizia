using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly MDBService _mdbService;

    public CartController(MDBService mdbService)
    {
        _mdbService = mdbService;
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<List<MDBOrders>>> GetOrder(int customerId)
    {
        var orders = await _mdbService.GetOrder(customerId);

        if (orders.Value == null || orders.Value.Count == 0)
            return NotFound();

        return Ok(orders.Value);
    }

    [HttpPut]
    public async Task<IActionResult> AddProducts([FromBody] MDBSingleOrderDto cart)
    {
        var result = await _mdbService.AddProducts(
            cart.ClientID,
            cart.Products,
            cart.qty
        );

        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Value);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteOrder([FromQuery] int customerId)
    {
        var result = await _mdbService.DeleteOrder(customerId);

        if (!result.Value)
            return NotFound();

        return NoContent();
    }
}
