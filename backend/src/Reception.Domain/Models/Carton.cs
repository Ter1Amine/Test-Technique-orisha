using Reception.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reception.Domain.Models
{
    public class Carton
    {
        public string CartonId { get; set; }
        public string PaletteId { get; set; }
        public Palette Palette { get; set; }
        public ICollection<Product> Products { get;  set; } = [];

        [NotMapped]
        public ReceiptStatus Status => GetReceiptStatus(Products.Select(product => product.IsReceived));

        private static ReceiptStatus GetReceiptStatus(IEnumerable<bool> receiptStates)
        {
            var states = receiptStates.ToList();
            if (states.Count == 0 || states.All(isReceived => !isReceived))
                return ReceiptStatus.NotReceived;

            return states.All(isReceived => isReceived)
                ? ReceiptStatus.Received
                : ReceiptStatus.PartiallyReceived;
        }
    }
}
