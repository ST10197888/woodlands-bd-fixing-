using System.ComponentModel.DataAnnotations;

namespace Woodlands_Prototype_Insy7315.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = "";

        [StringLength(1000)]
        public string Description { get; set; } = "";

        public string Image { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
