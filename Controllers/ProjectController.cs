using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Managr.Controllers
{
    public class ProjectController : Controller
    {
        // Baza de date
        private readonly ApplicationDbContext db;

        // Managerul de utilizatori
        private readonly UserManager<ApplicationUser> _userManager;

        // Managerul de roluri
        private readonly RoleManager<IdentityRole> _roleManager;

        ProjectController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager=userManager;
            _roleManager=roleManager;
        }

        public IActionResult Index()
        {
            var Proiecte = db.Projects
                             .Include("Organizer");
            
            ViewBag.Proiecte=Proiecte;

            return View();
        }
    }
}
