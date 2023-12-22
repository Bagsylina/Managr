using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Managr.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public TasksController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, 
            IWebHostEnvironment env)
        {
            db = context;

            _userManager = userManager;
            _roleManager = roleManager;

            _env = env;
        }

        [HttpGet]
        public IActionResult New()
        {
            Models.Task task = new Models.Task();

            if (TempData["ProjectId"] == null)
            {
                TempData["Message"] = "The project does not exist or you don't have privileges to add a task.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Projects/Index");
            }
            else
            {
                ViewBag.ProjectId = TempData["ProjectId"];
            }

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> New(Models.Task task, IFormFile TaskFile)
        {
            if(ModelState.IsValid)
            {
                if (TaskFile.Length > 0)
                {
                    var storagePath = Path.Combine(_env.WebRootPath, "files", TaskFile.FileName);
                    var databaseFileName = "/files/" + TaskFile.FileName;

                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await TaskFile.CopyToAsync(fileStream);
                    }

                    task.Multimedia = databaseFileName;
                }
    
                db.Tasks.Add(task);
                db.SaveChanges();

                TempData["Message"] = "Task" + task.Title + " was added";
                TempData["Alert"] = "alert-success";

                return Redirect("/Projects/Show/" + task.ProjectId);
            }
            else
            {
                ViewBag.Message = "Invalid task";
                ViewBag.Alert = "alert-danger";
                ViewBag.ProjectId = task.ProjectId;

                return View(task);
            }
        }

        public IActionResult Show(int id)
        {
            Models.Task task = db.Tasks.Include("Comments").Include("Comments.User").Include("UserTasks").Include("UserTasks.User")
                                .Where(tsk => tsk.Id == id)
                                .First();

            return View(task);
        }

        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.CreatedDate = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();

                return Redirect("/Tasks/Show/" + comment.TaskId);
            }
            else
            {
                Models.Task task = db.Tasks.Include("Comments")
                                .Where(tsk => tsk.Id == comment.TaskId)
                                .First();

                return View(task);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id) 
        {
            Models.Task task = db.Tasks.Find(id);

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id,  Models.Task requestTask)
        {
            Models.Task task = db.Tasks.Find(id);

            if(ModelState.IsValid)
            {
                task.Title = requestTask.Title;
                task.Description = requestTask.Description;
                task.Status = requestTask.Status;
                task.StartDate = requestTask.StartDate;
                task.Deadline = requestTask.Deadline;

                db.SaveChanges();

                TempData["message"] = "Task " + task.Title + " was edited";
                TempData["messageType"] = "alert-success";

                return Redirect("/Tasks/Show/" + id);
            }
            else
            {
                ViewBag.Message = "Invalid task";
                ViewBag.Alert = "alert-danger";

                return View(task);
            }
        }

        [HttpPost]
        public IActionResult EditStatus(int id, [FromForm] string newStatus) 
        {
            Models.Task task = db.Tasks.Find(id);

            if(ModelState.IsValid)
            {
                task.Status = newStatus;
                db.SaveChanges();

                TempData["message"] = "The status of task " + task.Title + " was edited";
                TempData["messageType"] = "alert-success";

                return Redirect("/Tasks/Show/" + id);
            }
            else
            {
                TempData["message"] = "Invalid task";
                TempData["messageType"] = "alert-danger";

                return Redirect("/Projects/Show/" + task.ProjectId);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Models.Task task = db.Tasks.Include("Comments")
                                .Where(tsk => tsk.Id == id)
                                .First();
            db.Tasks.Remove(task);
            db.SaveChanges();

            TempData["message"] = "Task-ul a fost sters.";
            TempData["messageType"] = "alert-success";

            return Redirect("/Projects/Show/" + task.ProjectId);
        }
    }
}
