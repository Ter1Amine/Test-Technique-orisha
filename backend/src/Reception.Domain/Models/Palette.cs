using Reception.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reception.Domain.Models
{
    public class Palette
    {
        public string PaletteId { get; set; }
        public string CommandId { get; set; }
        public Command Command { get; set; }
        public ICollection<Carton> Cartons { get; set; } = [];
        [NotMapped]
        public int ReceivedProducts => Cartons.Sum(carton => carton.ReceivedProducts);
        [NotMapped]
        public int TotalProducts => Cartons.Sum(carton => carton.TotalProducts);
        [NotMapped]
        public string ReceivedPercent => $"{ReceivedProducts}/{TotalProducts}";
        [NotMapped]
        public ReceiptStatus Status
        {
            get
            {
                var products = Cartons.SelectMany(carton => carton.Products).ToList();
                if (products.Count == 0 || products.All(product => !product.IsReceived))
                    return ReceiptStatus.NotReceived;

                return products.All(product => product.IsReceived)
                    ? ReceiptStatus.Received
                    : ReceiptStatus.PartiallyReceived;
            }
        }
    }
}
