namespace Woodlands_Prototype_Insy7315.Models
{
    public class FaqItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = ""; // Materials, Process, Services, Installation, Trade, Branches, Warranty
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
    }
}
