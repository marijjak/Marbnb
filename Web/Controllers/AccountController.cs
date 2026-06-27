using System;
using System.Web.Mvc;
using Web.Data;
using Web.Models;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["username"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = Database.FindByUsername(model.Username);
            if (user == null || user.IsDeleted || user.Password != model.Password)
            {
                ModelState.AddModelError("", "Incorrect username or password.");
                return View(model);
            }

            SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (Session["username"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (Database.FindByUsername(model.Username) != null)
            {
                ModelState.AddModelError("Username", "That username is already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth ?? DateTime.Today,
                Gender = model.Gender,
                Role = UserRole.Guest
            };

            Database.Users.Add(user);
            SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private void SignIn(User user)
        {
            Session["username"] = user.Username;
            Session["role"] = user.Role.ToString();
            Session["fullName"] = user.FullName;
        }
    }
}
