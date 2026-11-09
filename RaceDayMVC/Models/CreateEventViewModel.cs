using System.ComponentModel.DataAnnotations;
namespace RaceDayMVC.Models;
public class CreateEventViewModel {
 [Required, StringLength(200)] public string Name { get; set; } = "";
 public string? Description { get; set; }
 [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);
 [Required, StringLength(200)] public string Location { get; set; } = "";
 [Required, StringLength(50)] public string Distance { get; set; } = "";
 [Required, StringLength(50)] public string EventType { get; set; } = "Running";
 public IFormFile? BannerImage { get; set; }
}
