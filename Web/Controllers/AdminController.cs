using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Data;
using Web.Infrastructure;
using Web.Models;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    [RoleAuthorize("Administrator")]
    public class AdminController : Controller
    {
        public ActionResult Users(string firstName, string lastName, string role, DateTime? dobFrom, DateTime? dobTo, string sort)
        {
            var query = Database.Users.GetAll().Where(u => u.Role != UserRole.Administrator);

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(u => u.FirstName.IndexOf(firstName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(u => u.LastName.IndexOf(lastName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse(role, out UserRole parsedRole))
                query = query.Where(u => u.Role == parsedRole);
            if (dobFrom.HasValue)
                query = query.Where(u => u.DateOfBirth >= dobFrom.Value);
            if (dobTo.HasValue)
                query = query.Where(u => u.DateOfBirth <= dobTo.Value);

            switch (sort)
            {
                case "name_desc": query = query.OrderByDescending(u => u.FirstName); break;
                case "dob_asc": query = query.OrderBy(u => u.DateOfBirth); break;
                case "dob_desc": query = query.OrderByDescending(u => u.DateOfBirth); break;
                case "role_asc": query = query.OrderBy(u => u.Role); break;
                case "role_desc": query = query.OrderByDescending(u => u.Role); break;
                default: query = query.OrderBy(u => u.FirstName); break;
            }

            ViewBag.FFirst = firstName;
            ViewBag.FLast = lastName;
            ViewBag.FRole = role;
            ViewBag.FDobFrom = dobFrom?.ToString("yyyy-MM-dd");
            ViewBag.FDobTo = dobTo?.ToString("yyyy-MM-dd");
            ViewBag.FSort = sort;
            return View(query.ToList());
        }

        [HttpGet]
        public ActionResult CreateHost()
        {
            ViewBag.IsCreate = true;
            return View("UserForm", new AdminUserFormViewModel { Role = UserRole.Host });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHost(AdminUserFormViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError("Password", "Password is required.");
            if (Database.FindByUsername(model.Username) != null)
                ModelState.AddModelError("Username", "That username is already taken.");

            if (!ModelState.IsValid)
            {
                ViewBag.IsCreate = true;
                return View("UserForm", model);
            }

            Database.Users.Add(new User
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth ?? DateTime.Today,
                Gender = model.Gender,
                Role = UserRole.Host
            });

            TempData["ok"] = "Host added.";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public ActionResult EditUser(int id)
        {
            var user = Database.Users.GetById(id);
            if (user == null || user.Role == UserRole.Administrator)
                return HttpNotFound();

            ViewBag.IsCreate = false;
            return View("UserForm", new AdminUserFormViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(AdminUserFormViewModel model)
        {
            var user = Database.Users.GetById(model.Id);
            if (user == null || user.Role == UserRole.Administrator)
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.IsCreate = false;
                return View("UserForm", model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.DateOfBirth = model.DateOfBirth ?? user.DateOfBirth;
            user.Gender = model.Gender;
            if (!string.IsNullOrEmpty(model.Password))
                user.Password = model.Password;

            Database.Users.Update(user);
            TempData["ok"] = "User updated.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int id)
        {
            var user = Database.Users.GetById(id);
            if (user == null || user.Role == UserRole.Administrator)
            {
                TempData["err"] = "User not found.";
                return RedirectToAction("Users");
            }

            if (user.Role == UserRole.Guest)
            {
                var active = Database.Reservations.GetAll().Where(r =>
                    r.GuestUsername == user.Username &&
                    (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved)).ToList();
                foreach (var r in active)
                {
                    r.Status = ReservationStatus.Cancelled;
                    Database.Reservations.Update(r);
                }
            }

            Database.Users.SoftDelete(id);
            TempData["ok"] = "User deleted.";
            return RedirectToAction("Users");
        }

        public ActionResult Accommodations(string availability, string sort)
        {
            var query = Database.Accommodations.GetAll().AsEnumerable();

            if (availability == "available")
                query = query.Where(a => a.IsAvailable);
            else if (availability == "unavailable")
                query = query.Where(a => !a.IsAvailable);

            switch (sort)
            {
                case "name_asc": query = query.OrderBy(a => a.Name); break;
                case "name_desc": query = query.OrderByDescending(a => a.Name); break;
                case "price_asc": query = query.OrderBy(a => a.PricePerNight); break;
                case "price_desc": query = query.OrderByDescending(a => a.PricePerNight); break;
                case "date_asc": query = query.OrderBy(a => a.DatePosted); break;
                default: query = query.OrderByDescending(a => a.DatePosted); break;
            }

            ViewBag.FAvailability = availability;
            ViewBag.FSort = sort;
            return View(query.ToList());
        }

        [HttpGet]
        public ActionResult EditAccommodation(int id)
        {
            var acc = Database.Accommodations.GetById(id);
            if (acc == null)
                return HttpNotFound();

            return View("AccommodationForm", new AccommodationFormViewModel
            {
                Id = acc.Id,
                Name = acc.Name,
                Type = acc.Type,
                Description = acc.Description,
                Address = acc.Address,
                City = acc.City,
                PricePerNight = acc.PricePerNight,
                MaxGuests = acc.MaxGuests,
                IsAvailable = acc.IsAvailable
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccommodation(AccommodationFormViewModel model, HttpPostedFileBase image)
        {
            var acc = Database.Accommodations.GetById(model.Id);
            if (acc == null)
                return HttpNotFound();
            if (!ModelState.IsValid)
                return View("AccommodationForm", model);

            acc.Name = model.Name;
            acc.Type = model.Type;
            acc.Description = model.Description;
            acc.Address = model.Address;
            acc.City = model.City;
            acc.PricePerNight = model.PricePerNight;
            acc.MaxGuests = model.MaxGuests;
            acc.IsAvailable = model.IsAvailable;
            var newImage = FileUpload.Save(image, "accommodations");
            if (newImage != null)
                acc.ImagePath = newImage;

            Database.Accommodations.Update(acc);
            TempData["ok"] = "Listing updated.";
            return RedirectToAction("Accommodations");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAccommodation(int id)
        {
            var acc = Database.Accommodations.GetById(id);
            if (acc == null)
            {
                TempData["err"] = "Listing not found.";
                return RedirectToAction("Accommodations");
            }

            bool hasActive = Database.Reservations.GetAll().Any(r =>
                r.AccommodationId == id &&
                (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved));
            if (hasActive)
            {
                TempData["err"] = "This listing has active reservations and can't be deleted.";
                return RedirectToAction("Accommodations");
            }

            Database.Accommodations.SoftDelete(id);
            TempData["ok"] = "Listing deleted.";
            return RedirectToAction("Accommodations");
        }

        public ActionResult Reservations()
        {
            Database.CompletePastReservations();
            ViewBag.AccNames = Database.Accommodations.GetAll(true).ToDictionary(a => a.Id, a => a.Name);
            return View(Database.Reservations.GetAll().OrderByDescending(r => r.CheckIn).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReservation(int id)
        {
            var res = Database.Reservations.GetById(id);
            if (res == null || res.Status != ReservationStatus.Created)
            {
                TempData["err"] = "Only created reservations can be approved.";
                return RedirectToAction("Reservations");
            }
            res.Status = ReservationStatus.Approved;
            Database.Reservations.Update(res);
            TempData["ok"] = "Reservation approved.";
            return RedirectToAction("Reservations");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelReservation(int id)
        {
            var res = Database.Reservations.GetById(id);
            if (res == null || (res.Status != ReservationStatus.Created && res.Status != ReservationStatus.Approved))
            {
                TempData["err"] = "Only created or approved reservations can be cancelled.";
                return RedirectToAction("Reservations");
            }
            if ((res.CheckIn - DateTime.Now).TotalHours < 24)
            {
                TempData["err"] = "Reservations can only be cancelled at least 24h before check-in.";
                return RedirectToAction("Reservations");
            }
            res.Status = ReservationStatus.Cancelled;
            Database.Reservations.Update(res);
            TempData["ok"] = "Reservation cancelled.";
            return RedirectToAction("Reservations");
        }

        public ActionResult Reviews()
        {
            ViewBag.AccNames = Database.Accommodations.GetAll(true).ToDictionary(a => a.Id, a => a.Name);
            ViewBag.ReviewerNames = Database.Users.GetAll(true).ToDictionary(u => u.Username, u => u.FullName);
            return View(Database.Reviews.GetAll().OrderByDescending(r => r.CreatedAt).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReview(int id)
        {
            var review = Database.Reviews.GetById(id);
            if (review == null || review.Status != ReviewStatus.Created)
            {
                TempData["err"] = "Only created reviews can be approved.";
                return RedirectToAction("Reviews");
            }
            review.Status = ReviewStatus.Approved;
            Database.Reviews.Update(review);
            TempData["ok"] = "Review approved.";
            return RedirectToAction("Reviews");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectReview(int id)
        {
            var review = Database.Reviews.GetById(id);
            if (review == null || review.Status != ReviewStatus.Created)
            {
                TempData["err"] = "Only created reviews can be rejected.";
                return RedirectToAction("Reviews");
            }
            review.Status = ReviewStatus.Rejected;
            Database.Reviews.Update(review);
            TempData["ok"] = "Review rejected.";
            return RedirectToAction("Reviews");
        }
    }
}
