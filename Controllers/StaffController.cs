
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;

namespace YourApp.Controllers
{
    public class StaffController : Controller
    {
        private readonly ClothingShopDbContext  _context;

        public StaffController(ClothingShopDbContext context)
        {
            _context = context;
        }

        // GET: Staff - View staff list
        public async Task<IActionResult> Index()
        {
            var staffList = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new StaffViewModel
                {
                    UserID = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    RoleNames = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();

            return View(staffList);
        }

        // GET: Staff/Create - Display form to create new staff
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateStaffViewModel
            {
                AvailableRoles = await _context.Roles
                    .Select(r => new RoleSelectionViewModel
                    {
                        RoleID = r.RoleId,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        IsSelected = false
                    })
                    .ToListAsync(),
                SelectedRoleIds = new List<int>()
            };

            return View(viewModel);
        }

        // POST: Staff/Create - Process the creation of new staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    model.AvailableRoles = await _context.Roles
                        .Select(r => new RoleSelectionViewModel
                        {
                            RoleID = r.RoleId,
                            RoleName = r.RoleName,
                            Description = r.Description,
                            IsSelected = model.SelectedRoleIds != null && model.SelectedRoleIds.Contains(r.RoleId)
                        })
                        .ToListAsync();
                    return View(model);
                }

                // Hash the password
                string hashedPassword = HashPassword(model.Password);

                // Create new user
                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashedPassword,
                    Phone = model.Phone,
                    Address = model.Address,
                    CreatedAt = DateTime.Now,
                    Status = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Assign roles if selected
                if (model.SelectedRoleIds != null && model.SelectedRoleIds.Count > 0)
                {
                    foreach (var roleId in model.SelectedRoleIds)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = newUser.UserId,
                            RoleId = roleId,
                            AssignedDate = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.AvailableRoles = await _context.Roles
                .Select(r => new RoleSelectionViewModel
                {
                    RoleID = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsSelected = model.SelectedRoleIds != null && model.SelectedRoleIds.Contains(r.RoleId)
                })
                .ToListAsync();
            return View(model);
        }

        // GET: Staff/ManagePermissions/5 - Show permissions management form
        public async Task<IActionResult> ManagePermissions(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Get user's current role IDs
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            // Get all roles with selection status
            var allRoles = await _context.Roles
                .Select(r => new RoleSelectionViewModel
                {
                    RoleID = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsSelected = userRoleIds.Contains(r.RoleId)
                })
                .ToListAsync();

            var viewModel = new StaffPermissionViewModel
            {
                UserID = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                AvailableRoles = allRoles
            };

            return View(viewModel);
        }

        // POST: Staff/UpdatePermissions/5 - Process permission updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermissions(int userId, List<int> selectedRoles)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get current user roles
            var currentUserRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            // Remove roles that are not in the selected list
            foreach (var userRole in currentUserRoles)
            {
                if (selectedRoles == null || !selectedRoles.Contains(userRole.RoleId))
                {
                    _context.UserRoles.Remove(userRole);
                }
            }

            // Add newly selected roles
            if (selectedRoles != null)
            {
                var existingRoleIds = currentUserRoles.Select(ur => ur.RoleId).ToList();
                foreach (var roleId in selectedRoles)
                {
                    if (!existingRoleIds.Contains(roleId))
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = userId,
                            RoleId = roleId,
                            AssignedDate = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/ToggleStatus/5 - Enable/disable a staff member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = !user.Status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}