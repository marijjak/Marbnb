using System;
using Newtonsoft.Json;

namespace Web.Models
{
    // Entitet: Rezervacija
    public class Reservation : BaseEntity
    {
        // Strani ključevi
        public int AccommodationId { get; set; }      // Smeštajni objekat
        public string GuestUsername { get; set; }     // Gost koji rezerviše

        public DateTime CheckIn { get; set; }          // Datum prijave (check-in)
        public DateTime CheckOut { get; set; }         // Datum odjave (check-out)
        public int NumberOfGuests { get; set; }        // Broj gostiju
        public decimal TotalPrice { get; set; }        // Ukupna cena
        public ReservationStatus Status { get; set; } = ReservationStatus.Created;

        // Broj noćenja (izvedeno; ne čuva se u datoteci)
        [JsonIgnore]
        public int Nights => (CheckOut - CheckIn).Days;
    }
}
