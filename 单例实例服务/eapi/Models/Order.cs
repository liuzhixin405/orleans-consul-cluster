using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace eapi.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Sku { get; set; }
        public int Count { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreateTime { get; set; }

        public DateTime? ShipMentTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public DateTime? RejectedTime { get; set; }

        private Order(string sku, int count)
        {
            Sku = sku;
            Count = count;
            Status = OrderStatus.Created;
            CreateTime = DateTime.Now;
        }

        public static Order Create(string sku, int count)
        {
            return new Order(sku, count);
        }
    }
}
