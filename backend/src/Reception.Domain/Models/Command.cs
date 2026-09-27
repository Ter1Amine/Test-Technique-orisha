namespace Reception.Domain.Models
{
    public class Command
    {
        public string CommandId { get; set; }
        public int Status { get; set; }
        public ICollection<Palette> Palettes { get; set; } = new List<Palette>();
    }
}
