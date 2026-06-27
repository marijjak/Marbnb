using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Web.Models
{
    public class Accommodation : BaseEntity
    {
        public string Name { get; set; }
        public AccommodationType Type { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public string ImagePath { get; set; }
        public DateTime DatePosted { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string HostUsername { get; set; }

        [JsonIgnore]
        public List<Review> Reviews { get; set; } = new List<Review>();

        [JsonIgnore]
        public double AverageRating { get; set; }
    }
}
