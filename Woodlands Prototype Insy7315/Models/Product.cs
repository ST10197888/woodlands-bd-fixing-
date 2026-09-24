using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Woodlands_Prototype_Insy7315.Models
{
    public class Product
    {
        [Key]
        public string Id { get; set; } = "";

        [Required]
        public string Category { get; set; } = "";

        [Required]
        public string Title { get; set; } = "";

        public string Tagline { get; set; } = "";

        public string Description { get; set; } = "";

        public string Image { get; set; } = "";

        public string GalleryJson { get; set; } = "[]";

        public string FeaturesJson { get; set; } = "[]";

        public string FinishesJson { get; set; } = "[]";

        [NotMapped]
        public List<string> Gallery
        {
            get => ReadList(GalleryJson);
            set => GalleryJson = WriteList(value);
        }

        [NotMapped]
        public List<string> Features
        {
            get => ReadList(FeaturesJson);
            set => FeaturesJson = WriteList(value);
        }

        [NotMapped]
        public List<string> Finishes
        {
            get => ReadList(FinishesJson);
            set => FinishesJson = WriteList(value);
        }

        public string LeadTime { get; set; } = "";

        public string? Tag { get; set; }

        public string? Price { get; set; }

        [NotMapped]
        public string DisplayPrice
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Price))
                    return "Price on request";

                return Price;
            }
        }

        private static List<string> ReadList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json)
                       ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string WriteList(List<string>? values)
        {
            return JsonSerializer.Serialize(
                values ?? new List<string>());
        }
    }

    public class ProductCategory
    {
        public string Id { get; set; } = "";

        public string Label { get; set; } = "";

        public string Image { get; set; } = "";

        public int Count { get; set; }

        public string Description { get; set; } = "";

        public string Slug { get; set; } = "";
    }

    public class HeroSlide
    {
        public int Id { get; set; }

        public string Heading { get; set; } = "";

        public string Accent { get; set; } = "";

        public string Sub { get; set; } = "";

        public string Image { get; set; } = "";

        public string Cta { get; set; } = "";

        public string Link { get; set; } = "";
    }
}