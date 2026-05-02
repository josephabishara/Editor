using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.ClientViewModel
{
    public class AssistantDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        public string? ClientName { get; set; } // display only

        [Required]
        [MaxLength(200)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Photo")]
        public string? Photo { get; set; } = "uploads/sssistants/default-photo.png";

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        public int? ApplicationUserId { get; set; }
    }
}
