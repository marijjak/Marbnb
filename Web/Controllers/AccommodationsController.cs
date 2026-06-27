using System.Linq;
using System.Web.Mvc;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class AccommodationsController : Controller
    {
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

            var reviewerNames = approved
                .Select(r => r.ReviewerUsername)
                .Distinct()
                .ToDictionary(u => u, u =>
                {
                    var user = Database.FindByUsername(u);
                    return user != null ? user.FullName : u;
                });
            ViewBag.ReviewerNames = reviewerNames;

            return View(acc);
        }
    }
}
