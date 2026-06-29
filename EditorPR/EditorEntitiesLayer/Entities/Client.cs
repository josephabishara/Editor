using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Text;

namespace EditorEntitiesLayer.Entities
{
    public class Client : BaseEntity
    {

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
        public string? Photo { get; set; } = "uploads/clients/default-company.png";

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(200)]
        public string? Contact { get; set; }

        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        [MaxLength(500)]
        public string? Website { get; set; }

        public int ManagersLimitedNewsDays { get; set; } = 0;

        // Identity link — the ApplicationUser created for this client
        public int? ApplicationUserId { get; set; }

        [MaxLength(500)]
        public string? ReportCoverPdf { get; set; }


        // Navigation
        public ICollection<Assistant> AssistantList { get; set; } = new List<Assistant>();
        public ICollection<WebsiteCustomerCategory> WebsiteCategories { get; set; } = new List<WebsiteCustomerCategory>();
        public ICollection<PublicationCustomerCategory> PublicationCategories { get; set; } = new List<PublicationCustomerCategory>();
        public ICollection<ChannelCustomerCategory> ChannelCategories { get; set; } = new List<ChannelCustomerCategory>();
    }
}
