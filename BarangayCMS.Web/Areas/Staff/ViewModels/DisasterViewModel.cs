using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Staff.Models
{
    public class DisasterViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ang uri ng kalamidad ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumagpas sa 100 characters.")]
        [Display(Name = "Uri ng Kalamidad (Disaster Type)")]
        public string DisasterType { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumagpas sa 500 characters.")]
        [Display(Name = "Deskripsyon / Detalye")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumagpas sa 150 characters.")]
        [Display(Name = "Apektadong Lugar / Lokasyon")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ang petsa at oras ay kinakailangan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Petsa at Oras ng Pangyayari")]
        public DateTime DateOccurred { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(30)]
        [Display(Name = "Status ng Sitwasyon")]
        public string Status { get; set; } = "Active"; // Active, Controlled, Resolved, Cleared
    }

    // ⬇️ IDAGDAG ITONG DALAWANG CLASS SA BABA ⬇️
    public class ResidentSmsOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Purok { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class SmsAlertHistoryItem
    {
        public int Id { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string EmergencyType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RecipientGroup { get; set; } = string.Empty;
        public int RecipientCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Status { get; set; } = "Sent";
        public string SentBy { get; set; } = "Staff";
    }
}