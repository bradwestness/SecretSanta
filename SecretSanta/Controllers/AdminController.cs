using System.Web.Mvc;
using SecretSanta.Models;

namespace SecretSanta.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        //
        // GET: /Admin/Users
        public ActionResult Users()
        {
            EditUsersModel model = EditUsersModel.Load();
            return View(model);
        }

        //
        // POST: /Admin/AddUser
        [HttpPost]
        public ActionResult AddUser(AddUserModel model)
        {
            if (model.EmailConflict())
            {
                ModelState.AddModelError("NewUser.Email", "A user already exists with the specified E-Mail Address.");
            }

            if (ModelState.IsValid)
            {
                model.CreateAccount();
            }

            return RedirectToAction("Users");
        }

        //
        // POST: /Admin/EditUser
        [HttpPost]
        public ActionResult EditUser(EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                model.Save();
            }

            return RedirectToAction("Users");
        }

        //
        // POST: /Admin/DeleteUser
        [HttpPost]
        public ActionResult DeleteUser(EditUserModel model)
        {
            model.Delete();

            return RedirectToAction("Users");
        }
    }
}