using System.ComponentModel.DataAnnotations;

namespace EditorEntitiesLayer.Entities;

public class BaseEntity 
{
    [Key]
    public int Id { get; set; }
    public int? Writed { get; set; } = 0;
    public int? Deleted { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int CreateId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UpdateId { get; set; } = 0;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public int DeleteId { get; set; } = 0;
    public DateTime? DeletedAt { get; set; } 
  
}
