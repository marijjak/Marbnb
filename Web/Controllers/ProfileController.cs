using System;
using System.Linq;
using System.Web.Mvc;
using Web.Data;
using Web.Infrastructure;
using Web.Models;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    [RoleAuthorize]
    public class ProfileController : Controller
    {
        private string CurrentUsername => Session["username"] as string;

        public ActionResult Index(string status, string availability, string sort)
        {
            var user = Database.FindByUsername(CurrentUsername);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (user.Role == UserRole.Host)
            {
                var listings = Database.Accommodations.GetAll().Where(a => a.HostUsername == user.Username);

                if (availability == "available")
                    listings = listings.Where(a => a.IsAvailable);
                else if (availability == "unavailable")
                    listings = listings.Where(a => !a.IsAvailable);

                switch (sort)
                {
                    case "name_asc": listings = listings.OrderBy(a => a.Name); break;
                    case "name_desc": listings = listings.OrderByDescending(a => a.Name); break;
                    case "price_asc": listings = listings.OrderBy(a => a.PricePerNight); break;
                    case "price_desc": listings = listings.OrderByDescending(a => a.PricePerNight); break;
                    case "date_asc": listings = listings.OrderBy(a => a.DatePosted); break;
                    default: listings = listings.OrderByDescending(a => a.DatePosted); break;
                }

                ViewBag.Listings = listings.ToList();
                ViewBag.Availability = availability;
                ViewBag.Sort = sort;
            }

            if (user.Role == UserRole.Guest)
            {
                var reservations = Database.Reservations.GetAll()
                    .Where(r => r.GuestUsername == user.Username);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, out ReservationStatus parsed))
                    reservations = reservations.Where(r => r.Status == parsed);

                ViewBag.Reservations = reservations.OrderByDescending(r => r.CheckIn).ToList();
                ViewBag.AccNames = Database.Accommodations.GetAll(true).ToDictionary(a => a.Id, a => a.Name);
                ViewBag.StatusFilter = status;
                ViewBag.MyReviews = Database.Reviews.GetAll()
                    .Where(r => r.ReviewerUsername == user.Username)
                    .GroupBy(r => r.AccommodationId)
                    .ToDictionary(g => g.Key, g => g.First());
            }

            return View(user);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var user = Database.FindByUsername(CurrentUsername);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var vm = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = Database.FindByUsername(CurrentUsername);
            if (user == null)
                return RedirectToAction("Login", "Account");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.DateOfBirth = model.DateOfBirth ?? user.DateOfBirth;
            user.Gender = model.Gender;
            if (!string.IsNullOrEmpty(model.NewPassword))
                user.Password = model.NewPassword;

            if (user.Role == UserRole.Administrator)
                Database.SaveAdmins();
            else
                Database.Users.Update(user);

            Session["fullName"] = user.FullName;
            TempData["ok"] = "Profile updated.";
            return RedirectToAction("Index");
        }
    }
}
