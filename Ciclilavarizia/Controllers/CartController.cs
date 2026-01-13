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

            return Ok(new { data = mdbJson });
        }

        [HttpGet("{clientId}")]
        public async Task<List<MDBOrders>> GetOrders(int clientId)
        {
            return await _mdbService.GetOrder(clientId);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateOrder([FromBody] MDBOrderDto order)
        {

            await _mdbService.CreateOrder(order);
            return Ok(order.ClientID);
        }

        [HttpPut("{clientId}")]
        public async Task<IActionResult> AddProducts(int clientId, int product, int quantity)
        {

            await _mdbService.AddProducts(clientId, product, quantity);


            return NoContent();
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteOrder(int clientId)
        {

            await _mdbService.DeleteOrder(clientId);

            return NoContent();
        }

    }
}

