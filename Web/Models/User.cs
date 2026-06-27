using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Web.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }

        [JsonIgnore]
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        [JsonIgnore]
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }
}
