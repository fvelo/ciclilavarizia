using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SalesOrderDetailsController : ControllerBase
    {
        private readonly SalesOrderService _service;
        public SalesOrderDetailsController(SalesOrderService salesOrderService)
        {     
            _service = salesOrderService;
        }

        [HttpGet()]
        public async Task<ActionResult<List<SalesOrderDetail>>> AllDetails()
        {
            var details = await _service.GetDetailsAsync();
            return details.Value.Count > 0
                ? Ok(details.Value)
                : NotFound();
        }

        [HttpGet("{SalesOrderID}")]
        public async Task<ActionResult<List<SalesOrderDetailDto>>> GetMyDetails(int SalesOrderID)
        {

            var details = await _service.GetDetailAsync(SalesOrderID);
            if (details.Value == null || details.Value.Count <= 0)
            {
                return NotFound();
            }

            return Ok(details.Value);
        }

        [HttpPost("{SalesOrderHeaderID}")]
        public async Task<ActionResult<SalesOrderDetail>> AddSalesDetails([FromBody] SalesOrderDetailDto sales, int SalesOrderHeaderID)
        {

            var details = await _service.AddSalesDetailsAsync(sales, SalesOrderHeaderID);
            if(!details.IsSuccess)
            {
                return BadRequest(details.ErrorMessage);
            }
            else if(details.Value == 0)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetMyDetails), new { id = details.Value });
        }

        [HttpPut("{SalesOrderDetailID}")]
        public async Task<ActionResult<SalesOrderDetail>> ModifySalesDetails([FromBody] SalesOrderDetailDto detail, int SalesOrderDetailID)
        {

            var detailed = await _service.UpdateSalesDetailsAsync(detail, SalesOrderDetailID);
            if(!detailed.IsSuccess)
            {
                return BadRequest(detailed.ErrorMessage);
            }
            else if (detailed.Value == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{SalesOrderDetailsID}")]
        public async Task<ActionResult> DeleteSalesDetails(int SalesOrderDetailsID)
        {

            var details = await _service.DeleteSalesDetailsAsync(SalesOrderDetailsID);
            if (!details.Value)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}

