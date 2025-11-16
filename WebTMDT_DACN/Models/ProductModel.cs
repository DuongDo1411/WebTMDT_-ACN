using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTMDT_DACN.Repository.Validation;

namespace WebTMDT_DACN.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Tên Sản phẩm")]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Mô tả Sản phẩm")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Giá sản phẩm")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá sản phẩm phải là một số dương")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhãn hiệu")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhãn hiệu")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public int CategoryId { get; set; }

        public CategoryModel Category { get; set; }

        public BrandModel Brand { get; set; }

        public RatingModel Ratings { get; set; }

        public string Image { get; set; } 
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
        public int Quantity { get; set; }
        public int Soldout { get; set; }
    }
}
