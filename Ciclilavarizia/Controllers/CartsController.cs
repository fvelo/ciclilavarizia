using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CartsController : ControllerBase
{
    private readonly CartService _cartService;

    public CartsController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MdbCart>>> GetCarts()
    {
        var result = await _cartService.GetCarts();
        return Ok(result.Value);
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<List<MdbCart>>> GetCart(int customerId)
    {
        var result = await _cartService.GetCart(customerId);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCart(MdbCartDto cart)
    {
        var resultCreate = await _cartService.CreateCart(cart);
        if (!resultCreate.IsSuccess)
            return BadRequest(resultCreate.ErrorMessage);

        return Ok(cart.CustomerId);
    }

    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteCart(int customerId)
    {
        var result = await _cartService.DeleteCart(customerId);

        if (!result.IsSuccess)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{customerId}")]
    public async Task<IActionResult> UpdateCart(int customerId, MdbCartDto cart)
    {
        var result = await _cartService.UpdateCart(customerId, cart);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        if (result.Value == -1)
            return NotFound();

        return Ok(customerId);
    }
}
