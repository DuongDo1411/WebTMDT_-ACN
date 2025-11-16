using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
   
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _datacontext;

        public UserController(DataContext context , UserManager<AppUserModel> userManager , RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _datacontext = context;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _datacontext.Users
                                        join ur in _datacontext.UserRoles on u.Id equals ur.UserId
                                        join r in _datacontext.Roles on ur.RoleId equals r.Id
                                        select new { User = u, RoleName = r.Name }).ToListAsync();
            return View(usersWithRoles);
        }
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user,user.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createUser.Id;
                    var role = _roleManager.FindByIdAsync(user.RoleId);
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if(!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(user);
                    }
                    return RedirectToAction("Index" , "User");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm người dùng";
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
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
           if(string.IsNullOrEmpty(id))
           {
               return NotFound();
           }
           var user = await _userManager.FindByIdAsync(id);
           if(user == null) { 
               return NotFound();
           }
           var deleteResult = await _userManager.DeleteAsync(user);
           if (!deleteResult.Succeeded)
           {
               return View("Error");
               
           }
           TempData["success"] = "Xóa người dùng thành công";
           return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("Edit")]
        //public async Task<IActionResult> Edit(string id , AppUserModel user)
        //{
        //    var existingUser = await _userManager.FindByIdAsync(id);
        //    if(existingUser == null)
        //    {
        //        return NotFound();
        //    }
        //    if(ModelState.IsValid)
        //    {
        //        existingUser.UserName = user.UserName;
        //        existingUser.Email = user.Email;
        //        existingUser.PhoneNumber = user.PhoneNumber;
        //        existingUser.RoleId = user.RoleId;

        //        var updateUserResult = await _userManager.UpdateAsync(existingUser);
        //        if(updateUserResult.Succeeded)
        //        {
        //            return RedirectToAction("Index", "User");
        //        }
        //        else
        //        {
        //            foreach (var error in updateUserResult.Errors)
        //            {
        //                ModelState.AddModelError(string.Empty, error.Description);
        //            }
        //            return View(existingUser);  
        //        }
        //    }
        //    var roles = await _roleManager.Roles.ToListAsync();
        //    ViewBag.Roles = new SelectList(roles, "Id", "Name");

        //    TempData["error"] = "Model Validation Failed";
        //    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
        //    string errorMessage = string.Join("\n", errors);

        //    return View(existingUser);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                // Cập nhật user thông tin cá nhân
                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    // --- BẮT ĐẦU xử lý vai trò ---
                    // Lấy danh sách role hiện tại của user
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    // Xóa hết role cũ
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                    }

                    // Lấy tên role mới từ RoleId
                    var newRole = await _roleManager.FindByIdAsync(user.RoleId);
                    if (newRole != null)
                    {
                        await _userManager.AddToRoleAsync(existingUser, newRole.Name);
                    }
                    // --- KẾT THÚC xử lý vai trò ---

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model Validation Failed";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }

    }
}
