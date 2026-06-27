using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Web.Models
{
    // Entitet: Korisnik
    public class User : BaseEntity
    {
        public string Username { get; set; }        // jedinstveno korisničko ime
        public string Password { get; set; }
        public string FirstName { get; set; }       // Ime
        public string LastName { get; set; }        // Prezime
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }    // čuva se u formatu dd/MM/yyyy
        public Gender Gender { get; set; }           // Pol
        public UserRole Role { get; set; }           // Tip korisnika

        // Navigacione liste iz specifikacije.
        // [JsonIgnore] -> ne čuvaju se u datoteci (da se podaci ne dupliraju);
        // popunjava ih servisni sloj na osnovu stranog ključa kod dece
        // (Reservation.GuestUsername, Accommodation.HostUsername).
        [JsonIgnore]
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        [JsonIgnore]
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }
}
