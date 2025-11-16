using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Controllers
{
    
    public class BrandController : Controller
    {
        private readonly DataContext _datacontext;
        public BrandController(DataContext context)
        {
            _datacontext = context;
        }
        public async Task<IActionResult> Index(string Slug = "")
        {

            BrandModel brand = _datacontext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();

            if (brand == null) return RedirectToAction("Index");

            var productsByBrand = _datacontext.Products.Where(p => p.BrandId == brand.Id);

            return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
