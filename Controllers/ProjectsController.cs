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

        [NonAction]
        public void SetAdminRights()
        {
            ViewBag.UserId = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");
        }

        // All the projects that a user has access to
        public IActionResult Index()
        {
            // TODO: Change the condition to also return the projects the user is part of
            var Proiecte = db.Projects
                             .Include("Organizer")
                             .Where(proj => proj.OrganizerId == _userManager.GetUserId(User) ||
                                            User.IsInRole("Admin")
                                   );

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
        public IActionResult Show(int Id)
        {
            Project proj;
            try
            {
                 proj = db.Projects
                          .Include("Organizer")
                          .Include("Tasks")
                          .Where(HasPrivileges(Id))
                          .First();
            }
            catch (Exception)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to see it";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }
            
            SetAdminRights();

            return View(proj);
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

        // Edit form
        // HttpGet by default
        public IActionResult Edit(int Id)
        {
            Project project;
            try
            {
                project = db.Projects
                            .Where(HasPrivileges(Id))
                            .First();
            }
            catch (Exception)
            {
                TempData["Message"] = "You do not have edit privileges on the project or the project does not exist.";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            return View(project);
        }

        // Check if the form modifications are valid and save modified project
        [HttpPost]
        public IActionResult Edit(Project formProj)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "The modifications could not be saved";
                ViewBag.Alert = "alert-danger";

                return View();
            }

            Project project;
            try
            {
                project = db.Projects
                            .Where(HasPrivileges(formProj.Id))
                            .First();
            }
            catch(Exception)
            {
                TempData["Message"] = "You do not have edit privileges on the project or the project does not exist.";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            project.Name = formProj.Name;
            project.Description = formProj.Description;
            
            // TODO: Change the organizer id if the project changes ownership

            db.SaveChanges();

            TempData["Message"] = "The modifications have been saved";
            TempData["Alert"] = "alert-success";

            return RedirectToAction("Index");
        }

        // Check for rights and delete the project
        public IActionResult Delete(int Id)
        {
            Project project;
            try
            {
                project = db.Projects
                            .Include("Tasks") // This  should do delete on cascade
                            .Where(HasPrivileges(Id))
                            .First();
            }
            catch(Exception)
            {
                TempData["Message"] = "You do not have delete privileges on the project or the project does not exist.";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            db.Projects.Remove(project);
            db.SaveChanges();

            TempData["Message"] = "The project has been deleted successfuly";
            TempData["Alert"] = "alert-success";

            return RedirectToAction("Index");
        }

        // Form to add members
        // HttpGet by default
        public IActionResult AddMembers(int Id)
        {
            Project? project;

            project = GetProjectById(Id);
            if (project == null)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges over it.";
                TempData["Alert"] = "alert-danger";
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // Returns the lambda that checks if the user has privileges
        // over the project with Id given as parameter
        [NonAction]
        private Func<Project, bool> HasPrivileges(int Id)
        {
            return proj => proj.Id == Id && 
                           (User.IsInRole("Admin") ||
                            _userManager.GetUserId(User) == proj.OrganizerId
                           );
        }

        // Checks for Organizer/Admin privileges and checks if the project is in the database
        // If the project exists and the user has Organizer/Admin privileges then it is returned.
        // Otherwise null is returned
        [NonAction]
        public Project? GetProjectById(int Id)
        {
            try
            {
                Project? proj = db.Projects
                                  .Where(HasPrivileges(Id))
                                  .First();
                return proj;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
