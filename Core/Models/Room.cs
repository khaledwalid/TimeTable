using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTable.Core.Models;

public class Room
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int RoomId { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public string Type { get; set; } = null!;

    [Required] public int Capacity { get; set; }

    // Navigation property for slots
    public ICollection<Slot> Slots { get; set; } = null!;
    
    [Required]
    public string Zone { get; set; } = null!; // Male, Female, or Shared
    
}