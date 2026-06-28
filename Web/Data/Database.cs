using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Web.Models;

namespace Web.Data
{
    public static class Database
    {
        public static JsonStore<User> Users { get; private set; }
        public static JsonStore<Accommodation> Accommodations { get; private set; }
        public static JsonStore<Reservation> Reservations { get; private set; }
        public static JsonStore<Review> Reviews { get; private set; }
        public static List<User> Admins { get; private set; } = new List<User>();

        private static string DataPath(string file)
        {
            return HostingEnvironment.MapPath("~/App_Data/" + file);
        }

        public static void Initialize()
        {
            Users = new JsonStore<User>(DataPath("users.json"));
            Accommodations = new JsonStore<Accommodation>(DataPath("accommodations.json"));
            Reservations = new JsonStore<Reservation>(DataPath("reservations.json"));
            Reviews = new JsonStore<Review>(DataPath("reviews.json"));

            LoadAdmins();

            if (Users.IsEmpty() && Accommodations.IsEmpty())
                Seed();

            CompletePastReservations();
        }

        public static void CompletePastReservations()
        {
            if (Reservations == null)
                return;

            var all = Reservations.GetAll(true);
            var today = DateTime.Today;
            bool changed = false;

            foreach (var r in all)
            {
                if (!r.IsDeleted && r.Status == ReservationStatus.Approved && r.CheckOut < today)
                {
                    r.Status = ReservationStatus.Completed;
                    changed = true;
                }
            }

            if (changed)
                Reservations.SaveAll(all);
        }

        private static void LoadAdmins()
        {
            var path = DataPath("admins.json");
            if (!File.Exists(path))
            {
                Admins = new List<User>();
                return;
            }

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "dd/MM/yyyy",
                Converters = { new StringEnumConverter() }
            };

            var json = File.ReadAllText(path);
            Admins = JsonConvert.DeserializeObject<List<User>>(json, settings) ?? new List<User>();
            foreach (var a in Admins)
                a.Role = UserRole.Administrator;
        }

        public static void SaveAdmins()
        {
            File.WriteAllText(DataPath("admins.json"),
                JsonConvert.SerializeObject(Admins, JsonStore<User>.Settings));
        }

        public static User FindByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            var user = Users.GetAll().FirstOrDefault(x =>
                string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user != null)
                return user;

            return Admins.FirstOrDefault(x =>
                string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        private static void Seed()
        {
            var today = DateTime.Today;

            Users.Add(new User { Username = "marko", Password = "marko123", FirstName = "Marko", LastName = "Petrović", Email = "marko@marbnb.com", DateOfBirth = new DateTime(1988, 5, 12), Gender = Gender.Male, Role = UserRole.Host });
            Users.Add(new User { Username = "jovana", Password = "jovana123", FirstName = "Jovana", LastName = "Ilić", Email = "jovana@marbnb.com", DateOfBirth = new DateTime(1991, 9, 3), Gender = Gender.Female, Role = UserRole.Host });

            Users.Add(new User { Username = "ana", Password = "ana123", FirstName = "Ana", LastName = "Marković", Email = "ana@gmail.com", DateOfBirth = new DateTime(1997, 2, 20), Gender = Gender.Female, Role = UserRole.Guest });
            Users.Add(new User { Username = "nikola", Password = "nikola123", FirstName = "Nikola", LastName = "Jovanović", Email = "nikola@gmail.com", DateOfBirth = new DateTime(1995, 11, 8), Gender = Gender.Male, Role = UserRole.Guest });
            Users.Add(new User { Username = "milica", Password = "milica123", FirstName = "Milica", LastName = "Kovač", Email = "milica@gmail.com", DateOfBirth = new DateTime(1999, 7, 14), Gender = Gender.Female, Role = UserRole.Guest });

            var a1 = Accommodations.Add(new Accommodation { Name = "The Loft, Old Town", Type = AccommodationType.Apartment, Description = "Bright top-floor loft in the heart of the old town.", Address = "Zmaj Jovina 14", City = "Novi Sad", PricePerNight = 48m, MaxGuests = 4, ImagePath = null, DatePosted = today.AddDays(-60), IsAvailable = true, HostUsername = "marko" });
            var a2 = Accommodations.Add(new Accommodation { Name = "Villa Tara", Type = AccommodationType.Villa, Description = "Secluded villa with a view over the Tara canyon.", Address = "Kaluđerske Bare bb", City = "Bajina Bašta", PricePerNight = 82m, MaxGuests = 8, ImagePath = null, DatePosted = today.AddDays(-45), IsAvailable = true, HostUsername = "jovana" });
            var a3 = Accommodations.Add(new Accommodation { Name = "Hôtel Moskva Suite", Type = AccommodationType.Hotel, Description = "Elegant suite in a landmark hotel.", Address = "Balkanska 1", City = "Belgrade", PricePerNight = 110m, MaxGuests = 2, ImagePath = null, DatePosted = today.AddDays(-30), IsAvailable = true, HostUsername = "marko" });
            var a4 = Accommodations.Add(new Accommodation { Name = "Riverside Studio", Type = AccommodationType.Studio, Description = "Cozy studio steps from the Danube.", Address = "Beogradski kej 5", City = "Novi Sad", PricePerNight = 35m, MaxGuests = 2, ImagePath = null, DatePosted = today.AddDays(-20), IsAvailable = true, HostUsername = "jovana" });
            var a5 = Accommodations.Add(new Accommodation { Name = "Mountain Cottage", Type = AccommodationType.Cottage, Description = "Wooden cottage surrounded by pine forest.", Address = "Tornik 22", City = "Zlatibor", PricePerNight = 60m, MaxGuests = 6, ImagePath = null, DatePosted = today.AddDays(-15), IsAvailable = true, HostUsername = "marko" });
            var a6 = Accommodations.Add(new Accommodation { Name = "City Hostel", Type = AccommodationType.Hostel, Description = "Budget-friendly beds in central Belgrade.", Address = "Cara Dušana 9", City = "Belgrade", PricePerNight = 18m, MaxGuests = 3, ImagePath = null, DatePosted = today.AddDays(-10), IsAvailable = false, HostUsername = "jovana" });

            AddReservation("ana", a1, today.AddDays(-20), today.AddDays(-17), 2, ReservationStatus.Completed);
            AddReservation("nikola", a2, today.AddDays(10), today.AddDays(14), 5, ReservationStatus.Approved);
            AddReservation("milica", a3, today.AddDays(20), today.AddDays(22), 2, ReservationStatus.Created);
            AddReservation("ana", a5, today.AddDays(5), today.AddDays(8), 4, ReservationStatus.Cancelled);
            AddReservation("nikola", a1, today.AddDays(-40), today.AddDays(-37), 3, ReservationStatus.Completed);
            AddReservation("milica", a2, today.AddDays(-30), today.AddDays(-25), 4, ReservationStatus.Completed);

            Reviews.Add(new Review { AccommodationId = a1.Id, ReviewerUsername = "ana", Title = "Stunning city loft", Content = "Spotless, stylish and perfectly located. Would book again.", Rating = 5, Status = ReviewStatus.Approved, CreatedAt = today.AddDays(-16) });
            Reviews.Add(new Review { AccommodationId = a1.Id, ReviewerUsername = "nikola", Title = "Great location", Content = "Comfortable bed and amazing view, a bit noisy at night.", Rating = 4, Status = ReviewStatus.Created, CreatedAt = today.AddDays(-36) });
            Reviews.Add(new Review { AccommodationId = a2.Id, ReviewerUsername = "milica", Title = "Not for me", Content = "Hard to reach without a car.", Rating = 2, Status = ReviewStatus.Rejected, CreatedAt = today.AddDays(-24) });
        }

        private static void AddReservation(string guest, Accommodation acc, DateTime checkIn, DateTime checkOut, int guests, ReservationStatus status)
        {
            var nights = (checkOut - checkIn).Days;
            Reservations.Add(new Reservation
            {
                AccommodationId = acc.Id,
                GuestUsername = guest,
                CheckIn = checkIn,
                CheckOut = checkOut,
                NumberOfGuests = guests,
                TotalPrice = nights * acc.PricePerNight,
                Status = status
            });
        }
    }
}
