using Reception.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reception.Domain.Models
{
    public class Product
    {
        public string RefId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public bool IsReceived { get; set; }
        public string CartonId { get; set; }
        public Carton Carton { get; set; }
        [NotMapped]
        public ReceiptStatus Status
        {
            get
            {
                if (!IsReceived)
                    return ReceiptStatus.NotReceived;

                return ReceiptStatus.Received;
            }
        }
    }
}
