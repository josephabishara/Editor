using EditorViewModelLayer.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace EditorViewModelLayer.ClientViewModel
{
    public class ClientDTO
    {
        public int Id { get; set; }

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

        // Password only required on Create   
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Company Photo")]
        public UploadFileDTO? PhotoFile { get; set; }

        [Display(Name = "Company Photo")]
        public string? Photo { get; set; } = "uploads/clients/default-company.png";

        [Display(Name = "Website URL")]
        [Url]
        public string? Url { get; set; }

        [Display(Name = "Contact")]
        public string? Contact { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "News Per Day Limit")]
        [Range(0, 9999)]
        public int ManagersLimitedNewsDays { get; set; } = 0;

        public int? ApplicationUserId { get; set; }

        // Assistants list (read-only, shown in Details)
        public List<AssistantDTO> AssistantList { get; set; } = new();

        public List<WebsiteCustomerCategoryDTO> WebsiteCategories { get; set; } = new();
        public List<PublicationCustomerCategoryDTO> PublicationCategories { get; set; } = new();
        public List<ChannelCustomerCategoryDTO> ChannelCategories { get; set; } = new();
    }
}
