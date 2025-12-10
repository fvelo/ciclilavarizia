using Ciclilavarizia.Data;
using Ciclilavarizia.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // TODO: Create Error handleing and, add an actual error logging for Problem() response in every ActionMethod

        private CiclilavariziaDevContext _context;
        public ImagesController(CiclilavariziaDevContext context)
        {
            _context = context;
        }

        [HttpGet("product/{id}")]
        [EnsureProductExists(IdParameterName = "id")]
        public async Task<IActionResult> GetProductThumbnail(int id, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductID == id)
                .Select(p => new { p.ThumbNailPhoto, p.ThumbnailPhotoFileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (product?.ThumbNailPhoto == null) return NotFound();

            // Declaring magic string mime type TODO: understand if from extention
            var contentType = "image/jpeg";
            return File(product.ThumbNailPhoto, contentType);
        }
    }
}
