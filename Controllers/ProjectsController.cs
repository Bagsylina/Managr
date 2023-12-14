using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Managr.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        // Database
        private readonly ApplicationDbContext db;

        // Users manager
        private readonly UserManager<ApplicationUser> _userManager;

        // Roles manager
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProjectsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager=userManager;
            _roleManager=roleManager;
        }

        // All the projects that a user has access to
        public IActionResult Index()
        {
            var Proiecte = db.Projects
                             .Include("Organizer")
                             .Where(proj => proj.OrganizerId == _userManager.GetUserId(User) || User.IsInRole("Admin"));

            ViewBag.Proiecte=Proiecte;

            if (TempData.ContainsKey("Message"))
            {
                ViewBag.Message = TempData["Message"];
                if (TempData.ContainsKey("Alert"))
                {
                    ViewBag.Alert = TempData["Alert"];
                }
                else
                {
                    ViewBag.Alert = "alert-success";
                }
            }

            return View();
        }

        // Shows one speciffic project
        public IActionResult Show(int id)
        {
            var Proiect = db.Projects
                            .Include("Organizer")
                            .Where(proj => proj.OrganizerId == _userManager.GetUserId(User) || User.IsInRole("Admin"));

            return View(Proiect);
        }

        // New project form
        // HttpGet by default
        public IActionResult New()
        {
            Project project = new Project();

            return View(project);
        }

        // Check project from form model and save to database
        [HttpPost]
        public IActionResult New(Project proj)
        {
            if (ModelState.IsValid)
            {
                proj.OrganizerId = _userManager.GetUserId(User);
                proj.CreationDate = DateTime.Now;

                db.Projects.Add(proj);
                db.SaveChanges();

                TempData["Message"] = "Project created successfuly";
                TempData["Alert"] = "alert-success";

                return RedirectToAction("Index");
            }

            // Something regarding the model is not OK
            ViewBag.Message = "Project could not be created";
            ViewBag.Alert = "alert-danger";

            return View(proj);
        }
    }
}
