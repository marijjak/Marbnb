using System.Linq;
using System.Web.Mvc;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var approved = Reviews().Where(r => r.Status == ReviewStatus.Approved).ToList();
            var available = Database.Accommodations.GetAll()
                .Where(a => a.IsAvailable)
                .ToList();

            foreach (var acc in available)
            {
                var accReviews = approved.Where(r => r.AccommodationId == acc.Id).ToList();
                acc.AverageRating = accReviews.Any() ? accReviews.Average(r => r.Rating) : 0;
            }

            var all = Database.Accommodations.GetAll();
            ViewBag.TotalStays = all.Count;
            ViewBag.TotalCities = all.Select(a => a.City).Distinct().Count();
            ViewBag.AvgRating = approved.Any() ? approved.Average(r => r.Rating).ToString("0.0") : "5.0";
            ViewBag.TotalGuests = Database.Users.GetAll().Count(u => u.Role == UserRole.Guest);

            return View(available);
        }

        private static System.Collections.Generic.List<Review> Reviews()
        {
            return Database.Reviews.GetAll();
        }
    }
}
