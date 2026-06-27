using System;
using Newtonsoft.Json;

namespace Web.Models
{
    public class Reservation : BaseEntity
    {
        public int AccommodationId { get; set; }
        public string GuestUsername { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Created;

        [JsonIgnore]
        public int Nights => (CheckOut - CheckIn).Days;
    }
}
