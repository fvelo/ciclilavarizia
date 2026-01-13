using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ciclilavarizia.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SalesOrderHeaderController : ControllerBase
    {
        private readonly CiclilavariziaDevContext _context;
        private readonly SalesOrderService _service;
        public SalesOrderHeaderController(CiclilavariziaDevContext context, SalesOrderService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet()]
        public async Task<ActionResult<List<SalesOrderHeader>>> AllHeaders()
        {
            return await _service.AllHeaders();
        }

        [HttpGet("{CustomerID}")]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetMyHeader(int CustomerID)
        {

            var header = await _service.GetMyHeader(CustomerID);
            if (header.Count == 0)
            {
                return BadRequest("no header found for this customer ID");
            }
            return header;


        }

        [HttpPost()]
        public async Task<ActionResult<SalesOrderHeader>> AddSalesHeader([FromBody] SalesOrderHeaderDto sales)
        {

            var address = await _service.AddSalesHeader(sales);
            if (!address )
            {
                return BadRequest("No address found");
            }
            
            return Ok();
        }

        
        [HttpPut("{SalesOrderID}")]
        public async Task<ActionResult<SalesOrderHeader>> ModifySalesHeader([FromBody] SalesOrderHeaderDto header, int SalesOrderID)
        {

            var headered = await _service.ModifySalesHeader(header, SalesOrderID);

            if (!headered)
            {
                return BadRequest("no header found");
            }
            
            return Ok(header);
        }

        [HttpDelete("{SalesOrderID}")]
        public async Task<ActionResult> DeleteSalesHeader(int SalesOrderID)
        {

            var header = await _service.DeleteSalesHeader(SalesOrderID);
            
            if (!header)
                return BadRequest(" no records found ");

            return NoContent();
        }
    }
}

