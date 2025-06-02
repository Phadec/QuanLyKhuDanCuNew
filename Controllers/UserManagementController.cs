using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this for SelectList
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Helpers;
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index(string searchString, string role, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => 
                    (u.UserName != null && u.UserName.Contains(searchString)) || 
                    (u.Email != null && u.Email.Contains(searchString)) ||
                    u.HoTen.Contains(searchString));
            }

            // If a role filter is specified
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }

            int pageSize = 10;
            var users = await QuanLyKhuDanCu.Helpers.PaginatedList<ApplicationUser>.CreateAsync(
                query.OrderBy(u => u.HoTen), page, pageSize);

            // Get all roles for the dropdown
            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", role);

            // For each user, get their roles
            foreach (var user in users)
            {
                ViewData[$"Roles_{user.Id}"] = await _userManager.GetRolesAsync(user);
            }

            return View(users);
        }

        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = roles;

            return View(user);
        }

        // GET: UserManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    DiaChi = model.DiaChi,
                    CMND = model.CMND,
                    SoDienThoai = model.SoDienThoai,
                    NgayTao = DateTime.Now,
                    TrangThai = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    else
                    {
                        // Default role is Resident
                        await _userManager.AddToRoleAsync(user, "Resident");
                    }

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                HoTen = user.HoTen,
                NgaySinh = user.NgaySinh,
                DiaChi = user.DiaChi,
                CMND = user.CMND,
                SoDienThoai = user.SoDienThoai,
                TrangThai = user.TrangThai,
                Role = roles.FirstOrDefault()
            };

            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.HoTen = model.HoTen;
                user.NgaySinh = model.NgaySinh;
                user.DiaChi = model.DiaChi;
                user.CMND = model.CMND;
                user.SoDienThoai = model.SoDienThoai;
                user.TrangThai = model.TrangThai;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles if changed
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Update password if provided
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var resetResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                        
                        if (!resetResult.Succeeded)
                        {
                            foreach (var error in resetResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
                            return View(model);
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
            return View(model);
        }

        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Xóa người dùng thành công.";
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(user);
        }

        // GET: UserManagement/ManageRoles
        public async Task<IActionResult> ManageRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // GET: UserManagement/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: UserManagement/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.Name);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(ManageRoles));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Vai trò này đã tồn tại.");
                }
            }
            return View(model);
        }

        // POST: UserManagement/DeleteRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Check if there are users in this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                TempData["Error"] = $"Không thể xóa vai trò vì có {usersInRole.Count} người dùng đang có vai trò này.";
                return RedirectToAction(nameof(ManageRoles));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ManageRoles));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction(nameof(ManageRoles));
        }
    }
}
