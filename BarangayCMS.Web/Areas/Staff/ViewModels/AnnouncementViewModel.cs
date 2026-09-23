using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Kailangan para sa IFormFile

namespace BarangayCMS.Web.Areas.Staff.ViewModels
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Ang Pamagat (Title) ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Ang Pamagat ay hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Announcement Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang Nilalaman (Content) ay kinakailangan.")]
        [Display(Name = "Content / Details")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Date Posted")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        public string Category { get; set; } = "General";
        public string AuthorName { get; set; } = "Staff";
        public DateTime PublishDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public bool IsPinned { get; set; }

        // 🖼️ MGA PROPERTIES PARA SA LARAWAN (IMAGE UPLOAD & DISPLAY)
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Helper property para ma-check kung may larawan ang announcement
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
    }
}