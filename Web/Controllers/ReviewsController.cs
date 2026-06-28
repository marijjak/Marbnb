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
    [RoleAuthorize("Guest")]
    public class ReviewsController : Controller
    {
        private string Me => Session["username"] as string;

        [HttpGet]
        public ActionResult Create(int accommodationId)
        {
            var acc = Database.Accommodations.GetById(accommodationId, true);
            if (acc == null)
                return HttpNotFound();

            if (!HasCompletedStay(accommodationId))
            {
                TempData["err"] = "You can only review stays you've completed.";
                return RedirectToAction("Index", "Profile");
            }

            var existing = MyReviewFor(accommodationId);
            if (existing != null)
                return RedirectToAction("Edit", new { id = existing.Id });

            ViewBag.AccName = acc.Name;
            return View("Form", new ReviewFormViewModel { AccommodationId = accommodationId, Rating = 5 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewFormViewModel model, HttpPostedFileBase image)
        {
            var acc = Database.Accommodations.GetById(model.AccommodationId, true);
            if (acc == null)
                return HttpNotFound();

            if (!HasCompletedStay(model.AccommodationId))
            {
                TempData["err"] = "You can only review stays you've completed.";
                return RedirectToAction("Index", "Profile");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AccName = acc.Name;
                return View("Form", model);
            }

            Database.Reviews.Add(new Review
            {
                AccommodationId = model.AccommodationId,
                ReviewerUsername = Me,
                Title = model.Title,
                Content = model.Content,
                Rating = model.Rating,
                ImagePath = FileUpload.Save(image, "reviews"),
                Status = ReviewStatus.Created,
                CreatedAt = DateTime.Now
            });

            TempData["ok"] = "Review submitted. It will appear once approved.";
            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var review = Database.Reviews.GetById(id);
            if (review == null || review.ReviewerUsername != Me)
                return HttpNotFound();

            var acc = Database.Accommodations.GetById(review.AccommodationId, true);
            ViewBag.AccName = acc != null ? acc.Name : "";
            return View("Form", new ReviewFormViewModel
            {
                Id = review.Id,
                AccommodationId = review.AccommodationId,
                Title = review.Title,
                Content = review.Content,
                Rating = review.Rating
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReviewFormViewModel model, HttpPostedFileBase image)
        {
            var review = Database.Reviews.GetById(model.Id);
            if (review == null || review.ReviewerUsername != Me)
                return HttpNotFound();

            if (!ModelState.IsValid)
            {
                var acc = Database.Accommodations.GetById(review.AccommodationId, true);
                ViewBag.AccName = acc != null ? acc.Name : "";
                return View("Form", model);
            }

            review.Title = model.Title;
            review.Content = model.Content;
            review.Rating = model.Rating;
            var newImage = FileUpload.Save(image, "reviews");
            if (newImage != null)
                review.ImagePath = newImage;
            review.Status = ReviewStatus.Created;

            Database.Reviews.Update(review);
            TempData["ok"] = "Review updated. It will be reviewed again.";
            return RedirectToAction("Index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var review = Database.Reviews.GetById(id);
            if (review == null || review.ReviewerUsername != Me)
            {
                TempData["err"] = "Review not found.";
                return RedirectToAction("Index", "Profile");
            }

            Database.Reviews.SoftDelete(id);
            TempData["ok"] = "Review deleted.";
            return RedirectToAction("Index", "Profile");
        }

        private bool HasCompletedStay(int accommodationId)
        {
            return Database.Reservations.GetAll().Any(r =>
                r.GuestUsername == Me &&
                r.AccommodationId == accommodationId &&
                r.Status == ReservationStatus.Completed);
        }

        private Review MyReviewFor(int accommodationId)
        {
            return Database.Reviews.GetAll().FirstOrDefault(r =>
                r.ReviewerUsername == Me && r.AccommodationId == accommodationId);
        }
    }
}
