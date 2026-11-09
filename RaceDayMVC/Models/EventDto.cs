namespace RaceDayMVC.Models;
public class EventDto {
 public string Location { get; set; } = "";
 public string Distance { get; set; } = "";
 public string EventType { get; set; } = "";
 public string? BannerImage { get; set; }
 public int TotalEnrolments { get; set; }
 public int EventId { get; set; }
 public string Name { get; set; } = "";
 public string? Description { get; set; }
 public DateTime EventDate { get; set; }
 public string? OrganiserName { get; set; }
 public List<CategoryDto> Categories { get; set; } = new();
}
public class CategoryDto {
 public int CategoryId { get; set; }
 public string Name { get; set; } = "";
 public string CategoryType { get; set; } = "";
}
