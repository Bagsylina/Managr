using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
            Models.Task task = db.Tasks.Include("Comments").Include("Comments.User")
                                .Where(tsk => tsk.Id == id)
                                .First();

            //some of the assigned users to the task
            var assignedUsers = db.ApplicationUsers
                                .Join(db.UserTasks, au => au.Id, ut => ut.UserId,
                                (au, ut) => new
                                {
                                    au.Id,
                                    au.UserName,
                                    ut.TaskId
                                })
                                .Where(ut => ut.TaskId == id)
                                .OrderBy(au => au.UserName)
                                .Take(10);
           
            ViewBag.AssignedUsers = assignedUsers;

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

        public IActionResult ShowUsers(int id) //page to show list of all assgined users to a task and option to remove them
        {

            Models.Task task = db.Tasks
                                .Where(tsk => tsk.Id == id)
                                .First();

            //users assigned to the task
            var assignedUsers = db.ApplicationUsers
                                .Join(db.UserTasks, au => au.Id, ut => ut.UserId,
                                (au, ut) => new
                                {
                                    au.Id,
                                    au.UserName,
                                    ut.TaskId,
                                    UserTaskId = ut.Id
                                })
                                .Where(ut => ut.TaskId == id)
                                .OrderBy(au => au.UserName);

            ViewBag.AssignedUsers = assignedUsers;

            return View(task);
        }


        public IActionResult AssignUsers(int id) //page to show list of all project users that aren't assigned to the task and option to add them
        {
            Models.Task task = db.Tasks
                                .Where(tsk => tsk.Id == id)
                                .First();

            //users not assigned to the task
            var usersToAdd = from au in db.ApplicationUsers
                             where !(from ut in db.UserTasks
                                     select ut.UserId)
                                     .Contains(au.Id)
                            select au;

            ViewBag.UsersToAdd = usersToAdd;

            return View(task);
        }

        [HttpPost]
        public IActionResult AddUser([FromForm] UserTask userTask) //assigned a new user to a task
        {
            if(ModelState.IsValid)
            {
                //checking if he's not already assigned to the task
                if (db.UserTasks
                    .Where(ut => ut.TaskId == userTask.TaskId)
                    .Where(ut => ut.UserId == userTask.UserId)
                    .Count() > 0)
                {
                    TempData["message"] = "This user is already assigned to this task.";
                    TempData["messageType"] = "alert-danger";
                }

                else
                {
                    db.UserTasks.Add(userTask);
                    db.SaveChanges();
                }
            }
            return Redirect("/Tasks/AssignUsers/" + userTask.TaskId);
        }

        [HttpPost]
        public IActionResult RemoveUser(int id) //remove a user from a task
        {
            UserTask userTask = db.UserTasks.Where(ut => ut.Id == id).First();

            db.UserTasks.Remove(userTask);
            db.SaveChanges();

            return Redirect("/Tasks/ShowUsers/" + userTask.TaskId);
        }
    }
}
