using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var Conns = db.ProjectUsers
                          .Where(pu => pu.UserId == _userManager.GetUserId(User)
                                       || User.IsInRole("Admin"))
                          .Select(pu => pu.ProjectId)
                          .Distinct();

            var Proiecte = db.Projects
                             .Include("Organizer")
                             .Where(proj => Conns.Contains(proj.Id));

            ViewBag.Proiecte=Proiecte;
            
            LoadAlert();

            return View();
        }

        // Shows one speciffic project
        public IActionResult Show(int Id)
        {
            Project proj;

            LoadAlert();

            try
            {
                var members = db.ProjectUsers
                                .Where(pu => pu.ProjectId == Id)
                                .Select(pu => pu.UserId);

                proj = db.Projects
                         .Include("Organizer")
                         .Include("ProjectUsers")
                         .Where(proj => proj.Id == Id
                                        && (User.IsInRole("Admin")
                                            || members.Contains(_userManager.GetUserId(User))))
                         .First();

                //Ability to search tasks of a project

                var tasks = db.Tasks
                              .Where(t => t.ProjectId == proj.Id)
                              .OrderBy(t => t.Deadline)
                              .OrderBy(t => (t.Status == "Completed"));

                var search = "";

                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    tasks = tasks.Where(t => t.Title.Contains(search) || t.Description.Contains(search))
                                 .OrderBy(t => t.Deadline)
                                 .OrderBy(t => (t.Status == "Completed"));
                }

                ViewBag.SearchString = search;
                ViewBag.Tasks = tasks;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Show/" + Id + "/?search=" + search;
                }

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

                ProjectUser projUsr = new ProjectUser();
                projUsr.ProjectId = proj.Id;
                projUsr.UserId = proj.OrganizerId;
                db.ProjectUsers.Add(projUsr);
                Console.WriteLine(projUsr.UserId+" "+projUsr.ProjectId);
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

                return View(project);
            }
            catch (Exception)
            {
                TempData["Message"] = "You do not have edit privileges on the project or the project does not exist.";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }
        }

        // Check if the form modifications are valid and save modified project
        [HttpPost]
        public IActionResult Edit(Project formProj)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "The modifications could not be saved";
                ViewBag.Alert = "alert-danger";

                return View(formProj);
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
                            .Include("Tasks")// This should do delete on cascade
                            .Include("ProjectUsers")
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

        // Form to Add a user to the project.
        // HttpGet by default
        public IActionResult AddMember(int Id)
        {
            Project? proj = GetProjectById(Id);

            LoadAlert();

            if (proj == null)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to add members";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            var members = db.ProjectUsers
                            .Where(pu => pu.ProjectId == Id)
                            .Select(pu => pu.UserId);
            var dropDownNonMembers = new List<SelectListItem>();
            foreach (var user in db.Users
                                   .Select(u => new {u.Id, u.UserName}))
            {
                if (!members.Contains(user.Id))
                {
                    dropDownNonMembers.Add(new SelectListItem{ Text = user.UserName, Value = user.Id });
                }
            }

            proj.DropDownNonMembers = dropDownNonMembers;

            if (dropDownNonMembers.Count == 0)
            {
                TempData["Message"] = "No more members can be added (All the users are also members)";
                TempData["Alert"] = "alert-warning";

                return RedirectToAction("Admin", new { Id = proj.Id });
            }

            return View(proj);
        }

        // Receives data from form and saves it to the data base
        [HttpPost]
        public IActionResult AddMember([FromForm] int projId, [FromForm] string userId)
        {
            ProjectUser pu = new ProjectUser();
            pu.ProjectId = projId;
            pu.UserId = userId;

            try
            {
                db.ProjectUsers.Add(pu);
                db.SaveChanges();

                ApplicationUser? member = db.ApplicationUsers.Find(userId);
                if (member != null)
                {
                    TempData["Message"] = "Member " + member.UserName + " added successfully";
                    TempData["Alert"] = "alert-success";
                }

                return RedirectToAction("AddMember", projId);
            }
            catch(Exception)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to add a member";
                TempData["Alert"] = "alert-danger";
                
                return RedirectToAction("Index");
            }
        }

        // Form to Remove a member from the project.
        // HttpGet by default
        public IActionResult RemoveMember(int Id)
        {
            Project? proj = GetProjectById(Id);

            LoadAlert();

            if (proj == null)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to remove a user";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            var members = db.ProjectUsers
                            .Include("User")
                            .Where(pu => pu.ProjectId == Id)
                            .Select(pu => new { pu.UserId, UserName = (pu.User == null ? "" : pu.User.UserName) });
            var dropDownMembers = new List<SelectListItem>();
            foreach (var user in members)
            {
                if (user.UserId != proj.OrganizerId)
                {
                    dropDownMembers.Add(new SelectListItem{ Text = user.UserName, Value = user.UserId });
                }
            }

            proj.DropDownMembers = dropDownMembers;

            if (dropDownMembers.Count == 0)
            {
                TempData["Message"] = "No more members can be removed (Only the Organizer is left)";
                TempData["Alert"] = "alert-warning";

                return RedirectToAction("Admin", new { Id = proj.Id });
            }

            return View(proj);
        }

        // Receives data from form and removes the member
        [HttpPost]
        public IActionResult RemoveMember([FromForm] int projId, [FromForm] string userId)
        {
            Project? proj = GetProjectById(projId);

            if (proj == null)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to remove a user";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Show", projId);
            }

            ProjectUser? pu = db.ProjectUsers
                                .Find(projId, userId);

            if (pu == null)
            {
                TempData["Message"] = "User is not member of the Project";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Show", projId);
            }

            db.ProjectUsers
              .Remove(pu);
            db.SaveChanges();

            ApplicationUser? user = db.ApplicationUsers
                                      .Find(userId);

            if (user != null)
            {
                TempData["Message"] = "Member " + user.UserName + " removed successfully";
                TempData["Alert"] = "alert-success";
            }

            return RedirectToAction("RemoveMember", projId);
        }

        // Admin controlls area
        public IActionResult Admin(int Id)
        {
            Project? proj = GetProjectById(Id);    

            LoadAlert();

            if (proj == null)
            {
                TempData["Message"] = "Project does not exist or you don't have admin rights over it";
                TempData["Alert"] = "alert-danger";
                return RedirectToAction("Index");
            }

            return View(proj);
        }

        // Form to change project ownership
        // HttpGet by default
        public IActionResult TransferOwnership(int Id)
        {
            Project? proj = GetProjectById(Id);

            LoadAlert();

            if (proj == null)
            {
                TempData["Message"] = "The project does not exist or you don't have admin privileges over it";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Index");
            }

            var members = db.ProjectUsers
                            .Include("User")
                            .Where(pu => pu.ProjectId == Id)
                            .Select(pu => new { pu.UserId, UserName = (pu.User == null ? "" : pu.User.UserName) });
            var dropDownMembers = new List<SelectListItem>();
            foreach (var user in members)
            {
                if (user.UserId != proj.OrganizerId)
                {
                    dropDownMembers.Add(new SelectListItem{ Text = user.UserName, Value = user.UserId });
                }
            }

            proj.DropDownMembers = dropDownMembers;

            if (dropDownMembers.Count == 0)
            {
                TempData["Message"] = "No other member in project";
                TempData["Alert"] = "alert-warning";

                return RedirectToAction("Admin", new { Id = proj.Id });
            }

            return View(proj);
        }

        // Receives data from form and transfers project ownership
        [HttpPost]
        public IActionResult TransferOwnership([FromForm] int projId, [FromForm] string userId)
        {
            Project? proj = GetProjectById(projId);

            if (proj == null)
            {
                TempData["Message"] = "The project does not exist or you don't have owner privileges over it";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Show", projId);
            }

            ProjectUser? pu = db.ProjectUsers
                                .Find(projId, userId);

            if (pu == null)
            {
                TempData["Message"] = "User is not a member of the Project";
                TempData["Alert"] = "alert-danger";

                return RedirectToAction("Admin", projId);
            }

            proj.OrganizerId = userId;
            db.SaveChanges();

            ApplicationUser? user = db.ApplicationUsers
                                      .Find(userId);

            if (user != null)
            {
                TempData["Message"] = "Ownership of project " + proj.Name + " changed successfuly to " + user.UserName;
                TempData["Alert"] = "alert-success";
            }

            return RedirectToAction("Show", new { Id = projId });
        }

        // Loads the alert and the message from TempData into ViewBag
        [NonAction]
        public void LoadAlert()
        {
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
        }
    }
}
