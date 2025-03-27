using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

public class ViewProfileController : Controller
{
    private readonly ClothingShopDbContext _context;
    

    public ViewProfileController(ClothingShopDbContext context)
    {
        _context = context;
        
    }

    // GET: /ViewProfile/View/1
    public IActionResult View(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: /ViewProfile/Edit/1
    public IActionResult Edit(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: /ViewProfile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int UserId, string FullName, string Email, string Phone, string Address)
    {
        

        // Find the user
        var user = _context.Users.Find(UserId);

      

        try
        {
            // Update user properties
            user.FullName = FullName;
            user.Email = Email;
            user.Phone = Phone;
            user.Address = Address;

            _context.SaveChanges();
            

            // Redirect to view profile
            return RedirectToAction("View", new { id = UserId });
        }
        catch (Exception ex)
        {
           

            // Re-create the user object to send back to the view
            var userViewModel = new User
            {
                UserId = UserId,
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Address = Address
            };

            ModelState.AddModelError("", "Unable to save changes: " + ex.Message);
            return View(userViewModel);
        }
    }

    public IActionResult Settings()
    {
        return View();
    }
}