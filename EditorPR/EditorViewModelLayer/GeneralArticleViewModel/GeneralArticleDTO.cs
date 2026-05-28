using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EditorViewModelLayer.GeneralArticleViewModel
{
    public class GeneralArticleDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Website")]
        public int WebsiteId { get; set; }
        public string? WebsiteName { get; set; }   // display only

        [Required]
        [Display(Name = "Writer")]
        public int WriterId { get; set; }
        public string? WriterName { get; set; }    // display only

        [Display(Name = "Language")]
        [MaxLength(50)]
        public string? Language { get; set; }

        [Display(Name = "Article Branding")]
        [MaxLength(20)]
        public string? ArticleBranding { get; set; }   // Branded | Not Branded | N/A

        [Display(Name = "Headline Branding")]
        [MaxLength(20)]
        public string? HeadlineBranding { get; set; }  // Branded | Not Branded | N/A

        [Display(Name = "Picture in Article")]
        [MaxLength(10)]
        public string? PictureinArticle { get; set; }  // Yes | No

        [Display(Name = "AI Generated")]
        public bool Generation { get; set; }

        [Display(Name = "Article URL")]
        [MaxLength(1000)]
        [Url]
        public string? ArticleURL { get; set; }

        [Required]
        [Display(Name = "Title")]
        [MaxLength(500)]
        public string? Title { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Images")]
        public string? Images { get; set; }
    }
}
