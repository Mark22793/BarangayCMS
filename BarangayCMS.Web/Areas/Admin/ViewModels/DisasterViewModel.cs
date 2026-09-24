using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Admin.Models
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
        [Display(Name = "Petsa at Oras ng Pangyayari")]
        public DateTime DateOccurred { get; set; }

        [Required]
        [MaxLength(30)]
        [Display(Name = "Status ng Sitwasyon")]
        public string Status { get; set; } = "Active";
    }
}