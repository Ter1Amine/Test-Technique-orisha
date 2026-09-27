using Reception.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reception.Domain.Models
{
    public class Command
    {
        public string CommandId { get; set; }
        public ICollection<Palette> Palettes { get; set; } = [];
        [NotMapped]
        public int ReceivedProducts => Palettes.Sum(palette => palette.ReceivedProducts);
        [NotMapped]
        public int TotalProducts => Palettes.Sum(palette => palette.TotalProducts);
        [NotMapped]
        public string ReceivedPercent => $"{ReceivedProducts}/{TotalProducts}";
        [NotMapped]
        public ReceiptStatus Status
        {
            get
            {
                var palettes = Palettes.ToList();
                if (palettes.Count == 0 || palettes.All(palette => palette.Status == ReceiptStatus.NotReceived))
                    return ReceiptStatus.NotReceived;

                return palettes.All(palette => palette.Status == ReceiptStatus.Received)
                    ? ReceiptStatus.Received
                    : ReceiptStatus.PartiallyReceived;
            }
        }
    }
}