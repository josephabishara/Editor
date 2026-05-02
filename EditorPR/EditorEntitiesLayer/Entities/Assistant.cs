using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EditorEntitiesLayer.Entities; 

public class Assistant : BaseEntity
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Photo { get; set; } = "uploads/sssistants/default-photo.png";

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    // Identity link — the ApplicationUser created for this assistant
    public int? ApplicationUserId { get; set; }

    // Navigation
    [ForeignKey(nameof(ClientId))]
    public Client Client { get; set; } = null!;

}
