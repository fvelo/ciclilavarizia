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
    public async Task<ActionResult<List<MdbCartDto>>> GetCarts(CancellationToken cancellationToken = default)
    {
        var result = await _cartService.GetCartsAsync(cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<List<MdbCartDto>>> GetCart(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.GetCartAsync(customerId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCart(MdbCartDto cart, CancellationToken cancellationToken = default)
    {
        var resultCreate = await _cartService.CreateCartAsync(cart, cancellationToken);
        if (!resultCreate.IsSuccess)
            return BadRequest(resultCreate.ErrorMessage);

        return Ok(cart.CustomerId);
    }

    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteCart(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.DeleteCartAsync(customerId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{customerId}")]
    public async Task<IActionResult> UpdateCart(int customerId, MdbCartDto cart, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.UpdateCartAsync(customerId, cart, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        if (result.Value == -1)
            return NotFound();

        return Ok(customerId);
    }
}
