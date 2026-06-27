using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string name, string city, string type, decimal? minPrice, decimal? maxPrice, string sort)
        {
            var query = Database.Accommodations.GetAll().Where(a => a.IsAvailable);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(a => a.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(a => a.City.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(type, out AccommodationType parsedType))
                query = query.Where(a => a.Type == parsedType);
            if (minPrice.HasValue)
                query = query.Where(a => a.PricePerNight >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(a => a.PricePerNight <= maxPrice.Value);

            switch (sort)
            {
                case "name_asc": query = query.OrderBy(a => a.Name); break;
                case "name_desc": query = query.OrderByDescending(a => a.Name); break;
                case "price_asc": query = query.OrderBy(a => a.PricePerNight); break;
                case "price_desc": query = query.OrderByDescending(a => a.PricePerNight); break;
                case "date_asc": query = query.OrderBy(a => a.DatePosted); break;
                default: query = query.OrderByDescending(a => a.DatePosted); break;
            }

            var result = query.ToList();

            var approved = Database.Reviews.GetAll().Where(r => r.Status == ReviewStatus.Approved).ToList();
            foreach (var acc in result)
            {
                var accReviews = approved.Where(r => r.AccommodationId == acc.Id).ToList();
                acc.AverageRating = accReviews.Any() ? accReviews.Average(r => r.Rating) : 0;
            }

            var all = Database.Accommodations.GetAll();
            ViewBag.TotalStays = all.Count;
            ViewBag.TotalCities = all.Select(a => a.City).Distinct().Count();
            ViewBag.AvgRating = approved.Any() ? approved.Average(r => r.Rating).ToString("0.0") : "5.0";
            ViewBag.TotalGuests = Database.Users.GetAll().Count(u => u.Role == UserRole.Guest);

            ViewBag.FName = name;
            ViewBag.FCity = city;
            ViewBag.FType = type;
            ViewBag.FMin = minPrice;
            ViewBag.FMax = maxPrice;
            ViewBag.FSort = sort;
            ViewBag.IsFiltered = !string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(city)
                || !string.IsNullOrWhiteSpace(type) || minPrice.HasValue || maxPrice.HasValue;

            return View(result);
        }
    }
}
