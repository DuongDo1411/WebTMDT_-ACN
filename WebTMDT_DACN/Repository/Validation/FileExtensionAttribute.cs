using System.ComponentModel.DataAnnotations;

namespace WebTMDT_DACN.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensions = { ".jpg", ".jpeg", ".png"};
                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Chỉ chấp nhận file có định dạng .jpg, .jpeg, .png");
                }
            }
            return ValidationResult.Success;
        }
    }
}
