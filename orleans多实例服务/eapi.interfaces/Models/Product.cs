using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eapi.interfaces.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Sku { get; set; }
        public int Count { get; set; }

        private Product(string sku, int count)
        {
            Sku = sku;
            Count = count;
        }
        public static Product Create(string sku, int count)
        {
            return new Product(sku, count);
        }
    }
}
