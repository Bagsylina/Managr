using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Authorization;
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

        //Only the Project Organizer has the rights to add a Task
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public IActionResult New()
        {
            var projectId = TempData["ProjectId"];

            Models.Task task = new Models.Task();

            if (projectId == null)
            {
                TempData["Message"] = "The project does not exist.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Projects/Index");
            }
            else
            {
                Project project = db.Projects.Find(projectId);

                if(project.OrganizerId != _userManager.GetUserId(User))
                {
                    TempData["Message"] = "You don't have the rights to add a task.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Projects/Index");
                }
                else
                {
                    ViewBag.ProjectId = projectId;
                    return View(task);
                }
            }
        }

        //Only the Project Organizer has the rights to add a Task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<IActionResult> New(Models.Task task, IFormFile TaskFile)
        {
            if(ModelState.IsValid)
            {
                Project project = db.Projects.Find(task.ProjectId);

                if (project.OrganizerId == _userManager.GetUserId(User))
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

                    TempData["Message"] = "Task " + task.Title + " was added";
                    TempData["Alert"] = "alert-success";

                    return Redirect("/Projects/Show/" + task.ProjectId);
                }

                else
                {
                    TempData["Message"] = "You don't have the rights to add a task.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Projects/Index");
                }
            }
            else
            {
                ViewBag.Message = "Invalid task";
                ViewBag.Alert = "alert-danger";
                ViewBag.ProjectId = task.ProjectId;

                return View(task);
            }
        }

        [Authorize(Roles = "User,Admin")]
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

            SetAccessRights(id);

            return View(task);
        }

        [Authorize(Roles = "User,Admin")]
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

        //Only the Project Organizer has the rights to edit a Task
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public IActionResult Edit(int id) 
        {
            Models.Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);

            if (project.OrganizerId == _userManager.GetUserId(User))
            {
                return View(task);
            }
            else
            {
                TempData["Message"] = "You don't have the rights to edit this task.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Tasks/Show/" + task.Id);
            }
        }

        //Only the Project Organizer has the rights to edit a Task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id,  Models.Task requestTask)
        {
            Models.Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);

            if (ModelState.IsValid)
            {
                if (project.OrganizerId == _userManager.GetUserId(User))
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
                    TempData["Message"] = "You don't have the rights to edit this task.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Tasks/Show/" + task.Id);
                }
            }
            else
            {
                ViewBag.Message = "Invalid task";
                ViewBag.Alert = "alert-danger";

                return View(task);
            }
        }

        //The Project Organizer and the Users assigned to the Task have the right to edit the status of a Task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult EditStatus(int id, [FromForm] string newStatus) 
        {
            Models.Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);
            bool isAssigned = (db.UserTasks
                            .Where(ut => ut.TaskId == task.Id)
                            .Where(ut => ut.UserId == _userManager.GetUserId(User))
                            .Count() > 0);

            if (ModelState.IsValid)
            {
                if (isAssigned || project.OrganizerId == _userManager.GetUserId(User))
                {
                    task.Status = newStatus;
                    db.SaveChanges();

                    TempData["message"] = "The status of task " + task.Title + " was edited";
                    TempData["messageType"] = "alert-success";

                    return Redirect("/Tasks/Show/" + id);
                }
                else
                {
                    TempData["Message"] = "You don't have the rights to edit the status of this task.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Tasks/Show/" + id);
                }
            }
            else
            {
                TempData["message"] = "Invalid task";
                TempData["messageType"] = "alert-danger";

                return Redirect("/Projects/Show/" + task.ProjectId);
            }
        }

        //Only the Admin and Project Organizer have the right to delete a Task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Models.Task task = db.Tasks.Include("Comments")
                                .Where(tsk => tsk.Id == id)
                                .First();
            Project project = db.Projects.Find(task.ProjectId);

            if (project.OrganizerId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();

                TempData["message"] = "Task-ul a fost sters.";
                TempData["messageType"] = "alert-success";

                return Redirect("/Projects/Show/" + task.ProjectId);
            }
            else
            {
                TempData["Message"] = "You don't have the rights delete this task.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Tasks/Show/" + id);
            }
        }

        private void SetAccessRights(int id)
        {
            var curuser = _userManager.GetUserId(User);
            Models.Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);
            
            //current user id for ability to edit his own comments
            ViewBag.CurrentUser = curuser;

            //check if user is assigned to the task so we can show the option to change status of a task
            ViewBag.IsAssigned = (db.UserTasks
                            .Where(ut => ut.TaskId == task.Id)
                            .Where(ut => ut.UserId == curuser)
                            .Count() > 0);

            //check if user is admin for delete rights
            ViewBag.IsAdmin = User.IsInRole("Admin");

            //check if user is project organizer
            ViewBag.IsOrganizer = (project.OrganizerId == curuser);

            //check if there is at least one user assigned to the task
            ViewBag.NonZeroAssigned = (db.UserTasks
                                    .Where(ut => ut.TaskId == task.Id)
                                    .Count() > 0);
        }

        //You can search the users assigned to a task
        [Authorize(Roles = "User,Admin")]
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

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                assignedUsers = assignedUsers.Where(au => au.UserName.Contains(search)).OrderBy(au => au.UserName);
            }

            ViewBag.SearchString = search;
            ViewBag.AssignedUsers = assignedUsers;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Tasks/ShowUsers/" + id +"/?search=" + search;
            }

            SetAccessRights(id);

            return View(task);
        }

        //Can search users to add them to a task
        //Only the Project Organizer can assign users to a task
        [Authorize(Roles = "User,Admin")]
        public IActionResult AssignUsers(int id) //page to show list of all project users that aren't assigned to the task and option to add them
        {
            Models.Task task = db.Tasks
                                .Where(tsk => tsk.Id == id)
                                .First();
            Project project = db.Projects.Find(task.ProjectId);

            if (project.OrganizerId == _userManager.GetUserId(User))
            {
                //users not assigned to the task
                var usersToAdd = from au in db.ApplicationUsers
                                 where (from pu in db.ProjectUsers
                                        where pu.ProjectId == task.ProjectId
                                        select pu.UserId)
                                        .Contains(au.Id)
                                 select au;
                 usersToAdd = from au in usersToAdd
                              where !(from ut in db.UserTasks
                                      where ut.TaskId == task.Id
                                      select ut.UserId)
                                      .Contains(au.Id)
                              orderby au.UserName ascending
                              select au;

                var search = "";

                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    usersToAdd = usersToAdd.Where(au => au.UserName.Contains(search)).OrderBy(au => au.UserName);
                }

                ViewBag.SearchString = search;
                ViewBag.UsersToAdd = usersToAdd;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Tasks/AssignUsers/" + id + "/?search=" + search;
                }

                SetAccessRights(id);

                return View(task);
            }
            else
            {
                TempData["Message"] = "You don't have the rights assign new users to this task.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Tasks/Show/" + id);
            }
        }

        //Only the Project Organizer can assign users to a task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddUser([FromForm] UserTask userTask) //assigned a new user to a task
        {
            Models.Task task = db.Tasks
                                .Where(tsk => tsk.Id == userTask.TaskId)
                                .First();
            Project project = db.Projects.Find(task.ProjectId);

            if (ModelState.IsValid)
            {
                if (project.OrganizerId == _userManager.GetUserId(User))
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
                else
                {
                    TempData["Message"] = "You don't have the rights assign new users to this task.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Tasks/Show/" + userTask.TaskId);
                }
            }
            return Redirect("/Tasks/AssignUsers/" + userTask.TaskId);
        }

        //Only the Project Organizer can remove users from a task
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult RemoveUser(int id) //remove a user from a task
        {
            UserTask userTask = db.UserTasks.Where(ut => ut.Id == id).First();
            Models.Task task = db.Tasks
                                .Where(tsk => tsk.Id == userTask.TaskId)
                                .First();
            Project project = db.Projects.Find(task.ProjectId);

            if (project.OrganizerId == _userManager.GetUserId(User))
            {
                db.UserTasks.Remove(userTask);
                db.SaveChanges();

                return Redirect("/Tasks/ShowUsers/" + userTask.TaskId);
            }
            else
            {
                TempData["Message"] = "You don't have the rights assign new users to this task.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Tasks/Show/" + userTask.TaskId);
            }
        }
    }
}
