using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CartController : ControllerBase
    {
        private MDBService _mdbService;
        public CartController(MDBService mdbService)
        {
            _mdbService = mdbService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCarts()
        {
            var mdbJson = await _mdbService.GetAllCarts();

            if(!mdbJson.IsSuccess)
            {
                return Problem(detail: mdbJson.ErrorMessage);
            }

            return Ok(mdbJson.Value);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<List<MDBOrders>>> GetOrders(int customerId)
        {
            var orders = await _mdbService.GetOrder(customerId);
            if(!orders.IsSuccess)
            {
                return Problem(detail: orders.ErrorMessage);
            }

            return Ok(orders.Value);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateOrder([FromBody] MDBOrderDto order)
        {

            var orders = await _mdbService.CreateOrder(order);
            if(!orders.IsSuccess)
            {
                return Problem(detail: orders.ErrorMessage);
            }
            return Ok(orders.Value);
        }

        [HttpPut("{customerId}")]
        public async Task<IActionResult> AddProducts(int customerId, int product, int quantity)
        {

            var orders = await _mdbService.AddProducts(customerId, product, quantity);
            if(!orders.IsSuccess)
            {
                return Problem(detail: orders.ErrorMessage);
            }

            return Ok(orders.Value);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteOrder(int customerId)
        {

            await _mdbService.DeleteOrder(customerId);

            return NoContent();
        }

    }
}

