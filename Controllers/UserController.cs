using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task.Data;
using task.Models;

namespace task.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _db.Users.OrderBy(u => u.LastLoginTime).ThenBy(u => u.Id).AsNoTracking().ToListAsync();
            return View(users);
        }

        //block user
        [HttpPost]
        public async Task<IActionResult>Block(int[] ids)
        {
            if (ids.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
            foreach (var user in users)
            {
                user.Status = task.Models.User.UserStatus.Blocked;
            }
            await _db.SaveChangesAsync();
            TempData["Message"] = "Selected users have been blocked.";
            return RedirectToAction(nameof(Index));
        }

        //unblock user
        [HttpPost]
        public async Task<IActionResult> Unblock(int[] ids)
        {
            if (ids.Length == 0) return RedirectToAction(nameof(Index));
        
            var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
            foreach (var user in users)
            {
                
                if (user.Status == task.Models.User.UserStatus.Blocked)
                {                    
                    user.Status = task.Models.User.UserStatus.Active;
                
                }
            }
            await _db.SaveChangesAsync();
            TempData["Message"] = "Selected users have been unblocked.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int[] ids)
        {
            if (ids.Length == 0) return RedirectToAction(nameof(Index));
        
            var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
            _db.Users.RemoveRange(users);  //used for deletion 
            
            await _db.SaveChangesAsync();
            TempData["Message"] = "Selected users have been deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnverified()
        {
            var unverifiedUsers = await _db.Users.Where(u => u.Status == task.Models.User.UserStatus.Unverified).ToListAsync();
            _db.Users.RemoveRange(unverifiedUsers);

            await _db.SaveChangesAsync();
            TempData["Message"] = "All unverified users have been deleted.";
            return RedirectToAction(nameof(Index));

        }
    }
}