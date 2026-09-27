namespace Reception.Domain.Models
{
    public class Palette
    {
        public string PaletteId { get; set; }
        public string CommandId { get; set; }
        public Command Command { get; set; }
        public ICollection<Carton> Cartons { get; set; } = [];
    }
}
