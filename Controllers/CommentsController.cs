using Managr.Data;
using Managr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Managr.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;

            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                Comment comment = db.Comments.Find(id);
                if (comment.UserId == _userManager.GetUserId(User))
                {
                    return View(comment);
                }
                else
                {
                    TempData["Message"] = "You don't have the rights to edit this comment.";
                    TempData["Alert"] = "alert-danger";

                    return Redirect("/Tasks/Show/" + comment.TaskId);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "The comment doesn't exist.";
                TempData["Alert"] = "alert-danger";

                return Redirect("/Projects/Index/");
            }
        }

        //A comments can only be edited by it's user
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Comment requestComment)
        { 
            Comment comment = db.Comments.Find(id);

            if(ModelState.IsValid)
            {
                if (comment.UserId == _userManager.GetUserId(User))
                {
                    comment.Content = requestComment.Content;
                    db.SaveChanges();
                }
                else
                {
                    TempData["Message"] = "You don't have the rights to edit this comment.";
                    TempData["Alert"] = "alert-danger";
                }

                return Redirect("/Tasks/Show/" + comment.TaskId);
            }
            else
                return View(comment);
        }

        //A comment can be deleted by an admin or it's user
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
            }
            else
            {
                TempData["Message"] = "You don't have the rights to delete this comment.";
                TempData["Alert"] = "alert-danger";
            }

            return Redirect("/Tasks/Show/" + comment.TaskId);
        }
    }
}
