using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Staff.ViewModels
{
    public class ComplaintViewModel
    {
        public int ComplaintId { get; set; }

        // Primary Key Alias
        public int Id
        {
            get => ComplaintId;
            set => ComplaintId = value;
        }

        [Display(Name = "Complainant Resident")]
        public int? ResidentId { get; set; }

        [Required(ErrorMessage = "Ang pangalan ng Complainant ay kailangan.")]
        [Display(Name = "Complainant Name")]
        public string ComplainantName { get; set; } = string.Empty;

        // Alias para sa ResidentFullName na hinahanap ng Controller
        public string ResidentFullName
        {
            get => string.IsNullOrEmpty(ComplainantName) ? "Unknown Resident" : ComplainantName;
            set => ComplainantName = value;
        }

        [Required(ErrorMessage = "Ang pangalan ng Respondent ay kailangan.")]
        [Display(Name = "Respondent / Inirereklamo")]
        public string RespondentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ilarawan ang reklamo o insidente.")]
        [Display(Name = "Complaint Details / Description")]
        public string Details { get; set; } = string.Empty;

        // Alias para sa Subject / Description na hinahanap ng Controller
        public string Subject
        {
            get => Details;
            set => Details = value;
        }

        public string Description
        {
            get => Details;
            set => Details = value;
        }

        [Display(Name = "Incident Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime IncidentDate { get; set; } = DateTime.Now;

        // Alias para sa DateSubmitted na hinahanap ng Controller
        public DateTime DateSubmitted
        {
            get => IncidentDate;
            set => IncidentDate = value;
        }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Assigned Officer")]
        public string AssignedOfficer { get; set; } = string.Empty;

        [Display(Name = "Resolution Notes")]
        public string ActionTaken { get; set; } = string.Empty;

        // Dropdown Lists para sa Create at Edit Views
        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}