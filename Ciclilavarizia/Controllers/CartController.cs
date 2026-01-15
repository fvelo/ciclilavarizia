using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;
using System.Composition;

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
        public async Task<IActionResult> GetAllOrders()
        {
            var mdbJson = await _mdbService.GetAllOrders();

            if(!mdbJson.IsSuccess)
            {
                return NotFound(mdbJson.ErrorMessage);
            }

            return Ok(mdbJson.Value);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<List<MDBOrders>>> GetOrder(int customerId)
        {
            var orders = await _mdbService.GetOrder(customerId);

            if (orders.Value == null || orders.Value.Count <= 0)
            {
                return NotFound();
            }

            return Ok(orders.Value);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateOrder([FromBody] MDBOrderDto order)
        {

            var orders = await _mdbService.CreateOrder(order);
            if(orders.Value == 0)
            {
                return BadRequest(orders.ErrorMessage);
            }
            return Ok(orders.Value);
        }

        [HttpPut()]
        public async Task<IActionResult> AddProducts([FromBody]MDBSingleOrderDto cart)
        {

            var orders = await _mdbService.AddProducts(cart.ClientID, cart.Products, cart.qty);
            if(!orders.IsSuccess)
            {
                return BadRequest(orders.ErrorMessage);
            }

            return Ok(orders.Value);
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteOrder(int customerId)
        {

            var orders = await _mdbService.DeleteOrder(customerId);

            if(!orders.Value)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}

