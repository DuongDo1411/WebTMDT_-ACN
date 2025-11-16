using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTMDT_DACN.Repository.Validation;

namespace WebTMDT_DACN.Models
{
    public class ContactModel
    {   
        [Key]
        [Required(ErrorMessage = "Yêu cầu nhập tiêu đề Website")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập bản đồ")]
        public string Map { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập SĐT")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập thông tin liên hệ")]
        public string Description { get; set; }
        public string LogoImg { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
