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

            var a1 = Accommodations.Add(new Accommodation { Name = "Hotel Moskva", Type = AccommodationType.Hotel, Description = "An icon of Belgrade - elegant rooms in the landmark Art-Nouveau hotel on Terazije.", Address = "Terazije 20", City = "Belgrade", PricePerNight = 120m, MaxGuests = 2, ImagePath = Gallery("moskva").FirstOrDefault(), Images = Gallery("moskva"), DatePosted = today.AddDays(-60), IsAvailable = true, HostUsername = "marko" });
            var a2 = Accommodations.Add(new Accommodation { Name = "Hotel Pupin", Type = AccommodationType.Hotel, Description = "Modern hotel with sleek rooms and skyline views in the heart of Novi Sad.", Address = "Bulevar Mihajla Pupina 6", City = "Novi Sad", PricePerNight = 85m, MaxGuests = 3, ImagePath = Gallery("pupin").FirstOrDefault(), Images = Gallery("pupin"), DatePosted = today.AddDays(-52), IsAvailable = true, HostUsername = "jovana" });
            var a3 = Accommodations.Add(new Accommodation { Name = "Villa Tara", Type = AccommodationType.Villa, Description = "Hand-built wooden villa tucked into the pine forest of the Tara mountain.", Address = "Kaluđerske Bare bb", City = "Bajina Bašta", PricePerNight = 78m, MaxGuests = 6, ImagePath = Gallery("tara").FirstOrDefault(), Images = Gallery("tara"), DatePosted = today.AddDays(-45), IsAvailable = true, HostUsername = "marko" });
            var a4 = Accommodations.Add(new Accommodation { Name = "Zlatibor Chalet", Type = AccommodationType.Cottage, Description = "Cosy alpine chalet on Zlatibor with a wooden terrace and clean mountain air.", Address = "Naselje Kraljev Trg 4", City = "Zlatibor", PricePerNight = 65m, MaxGuests = 5, ImagePath = Gallery("zlatibor").FirstOrDefault(), Images = Gallery("zlatibor"), DatePosted = today.AddDays(-38), IsAvailable = true, HostUsername = "jovana" });
            var a5 = Accommodations.Add(new Accommodation { Name = "Kopaonik Lux Apartment", Type = AccommodationType.Apartment, Description = "Stylish ski-resort apartment just steps from the Kopaonik slopes.", Address = "Suvo Rudište bb", City = "Kopaonik", PricePerNight = 110m, MaxGuests = 4, ImagePath = Gallery("kopaonik").FirstOrDefault(), Images = Gallery("kopaonik"), DatePosted = today.AddDays(-30), IsAvailable = true, HostUsername = "marko" });
            var a6 = Accommodations.Add(new Accommodation { Name = "Petrovaradin Studio", Type = AccommodationType.Studio, Description = "Warm studio beneath the Petrovaradin fortress, a short walk from the centre.", Address = "Štrosmajerova 7", City = "Novi Sad", PricePerNight = 45m, MaxGuests = 2, ImagePath = Gallery("petrovaradin").FirstOrDefault(), Images = Gallery("petrovaradin"), DatePosted = today.AddDays(-22), IsAvailable = true, HostUsername = "jovana" });
            var a7 = Accommodations.Add(new Accommodation { Name = "Centar Apartment", Type = AccommodationType.Apartment, Description = "Bright, modern apartment in the very centre of Novi Sad.", Address = "Zmaj Jovina 14", City = "Novi Sad", PricePerNight = 55m, MaxGuests = 4, ImagePath = Gallery("novisad").FirstOrDefault(), Images = Gallery("novisad"), DatePosted = today.AddDays(-16), IsAvailable = true, HostUsername = "marko" });
            var a8 = Accommodations.Add(new Accommodation { Name = "Skadarlija Apartment", Type = AccommodationType.Apartment, Description = "Charming apartment on Belgrade's bohemian cobbled street of Skadarlija.", Address = "Skadarska 29", City = "Belgrade", PricePerNight = 60m, MaxGuests = 3, ImagePath = Gallery("skadarlija").FirstOrDefault(), Images = Gallery("skadarlija"), DatePosted = today.AddDays(-10), IsAvailable = true, HostUsername = "jovana" });
            var a9 = Accommodations.Add(new Accommodation { Name = "Waterfront Residence", Type = AccommodationType.Apartment, Description = "Sleek riverside apartment in the Belgrade Waterfront with floor-to-ceiling views of the Sava.", Address = "Bulevar Vudro Vilsona 12", City = "Belgrade", PricePerNight = 95m, MaxGuests = 4, ImagePath = Gallery("bgvoda").FirstOrDefault(), Images = Gallery("bgvoda"), DatePosted = today.AddDays(-6), IsAvailable = true, HostUsername = "marko" });

            AddReservation("ana", a1, today.AddDays(-20), today.AddDays(-17), 2, ReservationStatus.Completed);
            AddReservation("nikola", a2, today.AddDays(10), today.AddDays(14), 2, ReservationStatus.Approved);
            AddReservation("milica", a5, today.AddDays(20), today.AddDays(22), 3, ReservationStatus.Created);
            AddReservation("ana", a4, today.AddDays(5), today.AddDays(8), 4, ReservationStatus.Cancelled);
            AddReservation("nikola", a1, today.AddDays(-40), today.AddDays(-37), 2, ReservationStatus.Completed);
            AddReservation("milica", a3, today.AddDays(-30), today.AddDays(-25), 4, ReservationStatus.Completed);

            Reviews.Add(new Review { AccommodationId = a1.Id, ReviewerUsername = "ana", Title = "An unforgettable stay", Content = "Spotless, elegant and perfectly located. We would book the Moskva again in a heartbeat.", Rating = 5, Status = ReviewStatus.Approved, CreatedAt = today.AddDays(-16) });
            Reviews.Add(new Review { AccommodationId = a1.Id, ReviewerUsername = "nikola", Title = "Great location", Content = "Comfortable bed and a wonderful view, a little noisy at night.", Rating = 4, Status = ReviewStatus.Created, CreatedAt = today.AddDays(-36) });
            Reviews.Add(new Review { AccommodationId = a3.Id, ReviewerUsername = "milica", Title = "Not for me", Content = "Beautiful but hard to reach without a car.", Rating = 2, Status = ReviewStatus.Rejected, CreatedAt = today.AddDays(-24) });
        }

        private static List<string> Gallery(string key)
        {
            var dir = HostingEnvironment.MapPath("~/Content/img");
            if (dir == null || !Directory.Exists(dir))
                return new List<string>();

            return Directory.GetFiles(dir, key + "_*")
                .Select(f => "~/Content/img/" + Path.GetFileName(f))
                .OrderBy(p => p)
                .ToList();
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
