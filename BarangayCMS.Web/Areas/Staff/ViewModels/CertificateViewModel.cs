using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Staff.ViewModels
{
    public class CertificateViewModel
    {
        // Primary Key Alias/Properties
        public int CertificateId { get; set; }

        public int Id
        {
            get => CertificateId;
            set => CertificateId = value;
        }

        [Required(ErrorMessage = "Pumili ng residente.")]
        [Display(Name = "Resident Name")]
        public int ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        // Alias para sa ResidentFullName na hinahanap ng Views
        public string ResidentFullName
        {
            get => string.IsNullOrEmpty(ResidentName) ? "Unknown Resident" : ResidentName;
            set => ResidentName = value;
        }

        [Display(Name = "Fee Paid (₱)")]
        [DataType(DataType.Currency)]
        [Range(0, 100000, ErrorMessage = "Maglagay ng tamang halaga ng bayad.")]
        public decimal FeePaid { get; set; }

        [Display(Name = "Receipt Path")]
        public string? PaymentReceiptPath { get; set; }

        [Required(ErrorMessage = "Pumili ng uri ng sertipiko.")]
        [Display(Name = "Certificate Type")]
        public string CertificateType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ibigay ang layunin o dahilan.")]
        [Display(Name = "Purpose / Reason")]
        public string Purpose { get; set; } = string.Empty;

        [Display(Name = "Control Number")]
        public string ControlNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kailangan ang status.")]
        [Display(Name = "Request Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Date Requested")]
        [DataType(DataType.Date)]
        public DateTime DateRequested { get; set; } = DateTime.Now;

        [Display(Name = "Date Issued")]
        [DataType(DataType.Date)]
        public DateTime? DateIssued { get; set; }

        [Display(Name = "Issued By")]
        public string IssuedBy { get; set; } = string.Empty;

        // Property para sa dropdown list ng mga residente sa Create/Edit Views
        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}