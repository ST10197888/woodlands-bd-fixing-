namespace Woodlands_Prototype_Insy7315.Models
{
    public class Testimonial
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string Location { get; set; } = "";
        public int Rating { get; set; } = 5;
        public string Review { get; set; } = "";
        public string Project { get; set; } = "";
    }
}
