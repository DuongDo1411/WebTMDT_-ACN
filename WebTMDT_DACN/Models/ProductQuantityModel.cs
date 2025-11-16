using System.ComponentModel.DataAnnotations;

namespace WebTMDT_DACN.Models
{
    public class ProductQuantityModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Số lượng")]
        public int Quantity { get; set; }
       
        public long ProductId { get; set; }
        public DateTime DateCreated { get; set; }
        
    }
}
