using System.ComponentModel.DataAnnotations;

namespace Web.Models.ViewModels
{
    public class AccommodationFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Select a type.")]
        public AccommodationType Type { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, 1000000, ErrorMessage = "Price must be greater than 0.")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Max guests is required.")]
        [Range(1, 100, ErrorMessage = "Max guests must be between 1 and 100.")]
        public int MaxGuests { get; set; }

        public bool IsAvailable { get; set; }
    }
}
