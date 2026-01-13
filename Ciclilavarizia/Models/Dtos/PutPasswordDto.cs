using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class PutPasswordDto
    {
        [StringLength(20)]
        public string PlainPassword { get; set; }
    }
}
