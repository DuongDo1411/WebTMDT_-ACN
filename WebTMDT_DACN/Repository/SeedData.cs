using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;

namespace WebTMDT_DACN.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext  _context, RoleManager<IdentityRole> roleManager)
        {
             _context.Database.Migrate();
            if (!_context.Roles.Any()) // Chỉ chạy nếu bảng Roles đang trống
            {
                // Danh sách các Role bạn muốn tạo
                var roles = new[] { "SALES", "ADMIN", "PUBLISHER", "AUTHOR" };

                foreach (var roleName in roles)
                {
                    // Kiểm tra xem Role đã tồn tại chưa
                    if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        // Nếu chưa, tạo Role mới
                        var role = new IdentityRole(roleName);
                        role.NormalizedName = roleName; // Gán NormalizedName
                        roleManager.CreateAsync(role).GetAwaiter().GetResult();
                    }
                }
            }
            if (!_context.Products.Any())
            {
                CategoryModel macbook = new CategoryModel{Name = "Macbook", Description= "Macbook products", Slug= "macbook", Status= 1 };
                CategoryModel pc = new CategoryModel { Name = "PC", Description = "PC products", Slug = "pc", Status = 1 };

                BrandModel apple = new BrandModel { Name = "Apple", Description = "Apple products", Slug = "apple", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Description = "Samsung products", Slug = "samsung", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is the best", Price = 999.99M, Brand = apple, Category = macbook, Image = "macbook1.jpg" },
                    new ProductModel { Name = "PC", Slug = "pc", Description = "Con cho Cao Bang Bo PC", Price = 899.99M, Brand = samsung, Category = pc, Image = "boPC.jpg" }
                );

                _context.SaveChanges();
            }
        }
        
            
        
    }
}
