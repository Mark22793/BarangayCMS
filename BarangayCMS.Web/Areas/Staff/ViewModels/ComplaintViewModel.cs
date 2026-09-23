using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Staff.ViewModels
{
    public class ComplaintViewModel
    {
        public int ComplaintId { get; set; }

        public int Id
        {
            get => ComplaintId;
            set => ComplaintId = value;
        }

        [Display(Name = "Complainant Resident")]
        public int? ResidentId { get; set; }

        [Display(Name = "Complainant Name")]
        public string? ComplainantName { get; set; }

        public string ResidentFullName
        {
            get => string.IsNullOrEmpty(ComplainantName) ? "Walk-in Resident" : ComplainantName;
            set => ComplainantName = value;
        }

        [Display(Name = "Respondent / Inirereklamo")]
        public string? RespondentName { get; set; }

        [Display(Name = "Subject / Case Matter")]
        public string Subject { get; set; } = string.Empty;

        [Display(Name = "Complaint Details / Description")]
        public string Description { get; set; } = string.Empty;

        public string Details
        {
            get => Description;
            set => Description = value;
        }

        [Display(Name = "Incident Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime IncidentDate { get; set; } = DateTime.Now;

        public DateTime DateSubmitted
        {
            get => IncidentDate;
            set => IncidentDate = value;
        }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Assigned Officer")]
        public string? AssignedOfficer { get; set; }

        [Display(Name = "Resolution Notes")]
        public string? ActionTaken { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}