using System;
using System.Linq;
using System.Web.Mvc;
using Web.Data;
using Web.Infrastructure;
using Web.Models;

namespace Web.Controllers
{
    [RoleAuthorize("Guest")]
    public class ReservationsController : Controller
    {
        private string CurrentUsername => Session["username"] as string;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int accommodationId, DateTime? checkIn, DateTime? checkOut, int numberOfGuests)
        {
            var acc = Database.Accommodations.GetById(accommodationId);
            if (acc == null || acc.IsDeleted || !acc.IsAvailable)
            {
                TempData["err"] = "This stay is not available.";
                return RedirectToAction("Index", "Home");
            }

            if (!checkIn.HasValue || !checkOut.HasValue || checkOut.Value <= checkIn.Value)
            {
                TempData["err"] = "Choose a valid check-in and check-out date.";
                return Back(accommodationId);
            }
            if (checkIn.Value.Date < DateTime.Today)
            {
                TempData["err"] = "Check-in date can't be in the past.";
                return Back(accommodationId);
            }
            if (numberOfGuests < 1 || numberOfGuests > acc.MaxGuests)
            {
                TempData["err"] = $"This stay allows up to {acc.MaxGuests} guests.";
                return Back(accommodationId);
            }

            var approved = Database.Reservations.GetAll()
                .Where(r => r.AccommodationId == accommodationId && r.Status == ReservationStatus.Approved);
            bool overlaps = approved.Any(r => checkIn.Value < r.CheckOut && r.CheckIn < checkOut.Value);
            if (overlaps)
            {
                TempData["err"] = "Those dates overlap an existing approved booking.";
                return Back(accommodationId);
            }

            var nights = (checkOut.Value - checkIn.Value).Days;
            Database.Reservations.Add(new Reservation
            {
                AccommodationId = accommodationId,
                GuestUsername = CurrentUsername,
                CheckIn = checkIn.Value,
                CheckOut = checkOut.Value,
                NumberOfGuests = numberOfGuests,
                TotalPrice = nights * acc.PricePerNight,
                Status = ReservationStatus.Created
            });

            TempData["ok"] = "Reservation created. It's now pending approval.";
            return RedirectToAction("Index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var res = Database.Reservations.GetById(id);
            if (res == null || res.GuestUsername != CurrentUsername)
            {
                TempData["err"] = "Reservation not found.";
                return RedirectToAction("Index", "Profile");
            }
            if (res.Status != ReservationStatus.Created && res.Status != ReservationStatus.Approved)
            {
                TempData["err"] = "Only created or approved reservations can be cancelled.";
                return RedirectToAction("Index", "Profile");
            }
            if ((res.CheckIn - DateTime.Now).TotalHours < 24)
            {
                TempData["err"] = "Reservations can only be cancelled at least 24h before check-in.";
                return RedirectToAction("Index", "Profile");
            }

            res.Status = ReservationStatus.Cancelled;
            Database.Reservations.Update(res);
            TempData["ok"] = "Reservation cancelled.";
            return RedirectToAction("Index", "Profile");
        }

        private ActionResult Back(int accommodationId)
        {
            return RedirectToAction("Details", "Accommodations", new { id = accommodationId });
        }
    }
}
