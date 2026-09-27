namespace Reception.Domain.Models
{
    public class Carton
    {
        public string CartonId { get; set; }
        public string PaletteId { get; set; }
        public Palette Palette { get; set; }
        public ICollection<Product> Products { get; set; } = [];
    }
}
