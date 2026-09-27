namespace Reception.Domain.Models
{
    public class Command
    {
        public string CommandId { get; set; }
        public ICollection<Palette> Palettes { get; set; } = [];
    }
}
