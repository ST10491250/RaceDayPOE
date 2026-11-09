using System.ComponentModel.DataAnnotations;
namespace RaceDayApi.DTOs
{
    public class UserProfileDto
    {
        [Required, StringLength(100)] public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)] public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } // Maps to Organiser.ContactPhone or ignored for Participants
    }
}
