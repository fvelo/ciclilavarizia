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
            var headers = await _service.AllHeaders();
            if(!headers.IsSuccess)
                return NotFound(headers.ErrorMessage);
            
            return Ok(headers.Value);
        }

        [HttpGet("{CustomerID}")]
        public async Task<ActionResult<List<SalesOrderHeaderDto>>> GetMyHeader(int CustomerID)
        {

            var header = await _service.GetMyHeader(CustomerID);
            if (!header.IsSuccess)
            {
                return NotFound(header.ErrorMessage);
            }
            return Ok(header.Value);


        }

        [HttpPost()]
        public async Task<ActionResult<SalesOrderHeader>> AddSalesHeader([FromBody] SalesOrderHeaderDto sales)
        {

            var header = await _service.AddSalesHeader(sales);
            if (!header.IsSuccess)
            {
                return BadRequest(header.ErrorMessage);
            }
            else if(header.Value == 0)
            {
                return NotFound();
            }

            return CreatedAtAction("GetMyHeader", header.Value);
        }

        
        [HttpPut("{SalesOrderID}")]
        public async Task<ActionResult<SalesOrderHeader>> ModifySalesHeader([FromBody] SalesOrderHeaderDto header, int SalesOrderID)
        {

            var headered = await _service.ModifySalesHeader(header, SalesOrderID);

            if (!headered.IsSuccess)
            {
                return BadRequest(headered.ErrorMessage);
            }
            else if (headered.Value == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{SalesOrderID}")]
        public async Task<ActionResult> DeleteSalesHeader(int SalesOrderID)
        {

            var header = await _service.DeleteSalesHeader(SalesOrderID);
            
            if (!header.IsSuccess)
                return NotFound();
            if(!header.Value)
                return Problem();

            return NoContent();
        }
    }
}

