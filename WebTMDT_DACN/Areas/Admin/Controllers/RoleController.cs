using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    [Authorize(Roles = "ADMIN")]
    public class RoleController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext context , RoleManager<IdentityRole> roleManager)
        {
            _datacontext = context;
            _roleManager = roleManager;

        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _datacontext.Roles.OrderBy(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Handle missing Id
            }
            var role = await _roleManager.FindByIdAsync(id);

            return View(role);
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            if(!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return Redirect("Index");
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string Id)
        {
            if(string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(Id);
            if(role == null)
            {
                return NotFound();
            }
            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa vai trò thành công";
            }
            catch(Exception ex)
            {
                TempData["error"] = "Lỗi khi xóa vai trò";
            }
            return RedirectToAction("Index");
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Handle missing Id
            }
            if (ModelState.IsValid) // Validate model state before proceeding
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(); // Handle role not found
                }
                role.Name = model.Name; // Update role properties with model data
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Cập nhật Role thành công";
                    return RedirectToAction("Index"); // Redirect to the index action
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi khi cập nhật Role");
                }

            }
            return View(model ?? new IdentityRole { Id = id });
        }
    }
}
