using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Slider")]
    [Authorize(Roles = "ADMIN,PUBLISHER,AUTHOR")]
    public class SliderController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SliderController(DataContext context , IWebHostEnvironment webHostEnvironment)
        {
            _datacontext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _datacontext.Sliders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
         
            if (ModelState.IsValid)
            {
                
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }
                _datacontext.Add(slider);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm Slide thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm Slide";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(slider);
        }
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _datacontext.Sliders.FindAsync(Id);
            return View(slider);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {
            var slider_existed = _datacontext.Sliders.Find(slider.Id);
            if (ModelState.IsValid)
            {

                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider_existed.Image = imageName;
                }


                slider_existed.Name = slider.Name;
                slider_existed.Description = slider.Description;
                slider_existed.Status = slider.Status;


                _datacontext.Update(slider_existed);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Cập nhật slider thành công";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(slider);
        }
        [Route("Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            SliderModel sliders = await _datacontext.Sliders.FindAsync(Id);
            if (sliders == null)
            {
                return NotFound();
            }
            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            string oldImagePath = Path.Combine(uploadsDir, sliders.Image);
            try
            {
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi xóa ảnh cũ");
            }
            _datacontext.Sliders.Remove(sliders);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Xóa ảnh thành công";
            return RedirectToAction("Index");
        }

    }
}
