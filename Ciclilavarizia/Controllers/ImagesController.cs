using Ciclilavarizia.Data;
using Ciclilavarizia.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : Controller
    {
        private CiclilavariziaDevContext _context;
        public ImagesController(CiclilavariziaDevContext context)
        {
            _context = context;
        }

        [EnsureProductExists]
        [HttpGet("/product/{id}")]
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
