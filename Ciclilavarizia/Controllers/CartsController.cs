using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Retrieves a list of all active shopping carts in the system.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <returns>A list of cart data transfer objects.</returns>
    /// <response code="200">Returns the list of carts.</response>
    [HttpGet]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<List<MdbCartDto>>> GetCarts(CancellationToken cancellationToken = default)
    {
        var result = await _cartService.GetCartsAsync(cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves the shopping cart associated with a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <returns>The cart details for the specified customer.</returns>
    /// <response code="200">Returns the requested cart.</response>
    /// <response code="404">If no cart is found for the given customer ID.</response>
    [HttpGet("{customerId}")]
    public async Task<ActionResult<List<MdbCartDto>>> GetCart(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.GetCartByCustomerIdAsync(customerId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound();

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new shopping cart for a customer.
    /// </summary>
    /// <param name="cart">The cart details to be created.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <returns>The ID of the customer the cart was created for.</returns>
    /// <response code="200">If the cart was successfully created.</response>
    /// <response code="400">If the cart data is invalid or creation fails.</response>
    [HttpPost]
    public async Task<IActionResult> CreateCart(MdbCartDto cart, CancellationToken cancellationToken = default)
    {
        var resultCreate = await _cartService.CreateCartAsync(cart, cancellationToken);
        if (!resultCreate.IsSuccess)
            return BadRequest(resultCreate.ErrorMessage);

        return Ok(cart.CustomerId);
    }

    /// <summary>
    /// Deletes a customer's shopping cart.
    /// </summary>
    /// <param name="customerId">The ID of the customer whose cart should be removed.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <response code="204">The cart was successfully deleted.</response>
    /// <response code="404">If the cart could not be found.</response>
    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteCart(int customerId, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.DeleteCartByCustomerIdAsync(customerId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Updates an existing shopping cart's contents.
    /// </summary>
    /// <param name="customerId">The ID of the customer whose cart is being updated.</param>
    /// <param name="cart">The updated cart data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <returns>The ID of the customer associated with the updated cart.</returns>
    /// <response code="200">The cart was successfully updated.</response>
    /// <response code="400">If the update logic fails validation.</response>
    /// <response code="404">If the cart to update does not exist.</response>
    [HttpPut("{customerId}")]
    public async Task<IActionResult> UpdateCart(int customerId, MdbCartDto cart, CancellationToken cancellationToken = default)
    {
        var result = await _cartService.UpdateCartByCostumerIdAsync(customerId, cart, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        if (result.Value == -1)
            return NotFound();

        return Ok(customerId);
    }
}
