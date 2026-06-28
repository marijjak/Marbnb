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
    public class AccommodationsController : Controller
    {
        private string Me => Session["username"] as string;

        public ActionResult Details(int id)
        {
            var acc = Database.Accommodations.GetById(id);
            if (acc == null || acc.IsDeleted)
                return HttpNotFound();

            var approved = Database.Reviews.GetAll()
                .Where(r => r.AccommodationId == id && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            acc.Reviews = approved;
            acc.AverageRating = approved.Any() ? approved.Average(r => r.Rating) : 0;

            var host = Database.FindByUsername(acc.HostUsername);
            ViewBag.HostName = host != null ? host.FullName : acc.HostUsername;

            ViewBag.ReviewerNames = approved
                .Select(r => r.ReviewerUsername)
                .Distinct()
                .ToDictionary(u => u, u =>
                {
                    var user = Database.FindByUsername(u);
                    return user != null ? user.FullName : u;
                });

            return View(acc);
        }

        [RoleAuthorize("Host")]
        [HttpGet]
        public ActionResult Create()
        {
            return View("Form", new AccommodationFormViewModel { IsAvailable = true, MaxGuests = 2 });
        }

        [RoleAuthorize("Host")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccommodationFormViewModel model, HttpPostedFileBase image)
        {
            if (!FileUpload.IsValidImage(image))
                ModelState.AddModelError("", "An image of the property is required.");
            if (!ModelState.IsValid)
                return View("Form", model);

            Database.Accommodations.Add(new Accommodation
            {
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Address = model.Address,
                City = model.City,
                PricePerNight = model.PricePerNight,
                MaxGuests = model.MaxGuests,
                IsAvailable = model.IsAvailable,
                HostUsername = Me,
                DatePosted = DateTime.Today,
                ImagePath = FileUpload.Save(image, "accommodations")
            });

            TempData["ok"] = "Listing created.";
            return RedirectToAction("Index", "Profile");
        }

        [RoleAuthorize("Host")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var acc = Database.Accommodations.GetById(id);
            if (acc == null || acc.HostUsername != Me)
                return HttpNotFound();
            if (!acc.IsAvailable)
            {
                TempData["err"] = "Unavailable listings can't be edited.";
                return RedirectToAction("Index", "Profile");
            }

            return View("Form", new AccommodationFormViewModel
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

        [RoleAuthorize("Host")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AccommodationFormViewModel model, HttpPostedFileBase image)
        {
            var acc = Database.Accommodations.GetById(model.Id);
            if (acc == null || acc.HostUsername != Me)
                return HttpNotFound();
            if (!acc.IsAvailable)
            {
                TempData["err"] = "Unavailable listings can't be edited.";
                return RedirectToAction("Index", "Profile");
            }
            if (!ModelState.IsValid)
                return View("Form", model);

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
            return RedirectToAction("Index", "Profile");
        }

        [RoleAuthorize("Host")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var acc = Database.Accommodations.GetById(id);
            if (acc == null || acc.HostUsername != Me)
            {
                TempData["err"] = "Listing not found.";
                return RedirectToAction("Index", "Profile");
            }
            if (!acc.IsAvailable)
            {
                TempData["err"] = "Unavailable listings can't be deleted.";
                return RedirectToAction("Index", "Profile");
            }

            bool hasActive = Database.Reservations.GetAll().Any(r =>
                r.AccommodationId == id &&
                (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved));
            if (hasActive)
            {
                TempData["err"] = "This listing has active reservations and can't be deleted.";
                return RedirectToAction("Index", "Profile");
            }

            Database.Accommodations.SoftDelete(id);
            TempData["ok"] = "Listing deleted.";
            return RedirectToAction("Index", "Profile");
        }
    }
}
