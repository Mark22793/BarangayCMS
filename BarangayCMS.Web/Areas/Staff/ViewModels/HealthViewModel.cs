using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Web.Areas.Staff.ViewModels
{
    public class HealthViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pumili ng residente.")]
        [Display(Name = "Patient / Resident")]
        public int ResidentId { get; set; }

        public string ResidentName { get; set; } = string.Empty;

        [Display(Name = "Weight (kg)")]
        [Range(0.1, 500.0, ErrorMessage = "Maglagay ng tamang timbang (kg).")]
        public double WeightKg { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(1.0, 300.0, ErrorMessage = "Maglagay ng tamang taas (cm).")]
        public double HeightCm { get; set; }

        [Display(Name = "Blood Type")]
        public string? BloodType { get; set; }

        [Display(Name = "Health Classification")]
        public string? HealthClassification { get; set; } = "General";

        [Display(Name = "Vaccination Status")]
        public bool IsVaccinated { get; set; }

        [Display(Name = "Medical Condition / Diagnosis")]
        public string? MedicalCondition { get; set; }

        [Display(Name = "Attending Health Worker")]
        public string? AttendingHealthWorker { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Date Recorded")]
        [DataType(DataType.Date)]
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        [Display(Name = "Last Checkup Date")]
        [DataType(DataType.Date)]
        public DateTime LastCheckupDate { get; set; } = DateTime.Now;

        // --- AUTOMATIC BMI COMPUTATIONS ---

        [Display(Name = "BMI")]
        public double Bmi
        {
            get
            {
                if (HeightCm > 0 && WeightKg > 0)
                {
                    // Formula: Weight (kg) / (Height (m))^2
                    double heightInMeters = HeightCm / 100.0;
                    return Math.Round(WeightKg / (heightInMeters * heightInMeters), 1);
                }
                return 0;
            }
        }

        [Display(Name = "BMI Category")]
        public string BmiCategory
        {
            get
            {
                if (Bmi <= 0) return "N/A";
                if (Bmi < 18.5) return "Underweight";
                if (Bmi < 25.0) return "Normal";
                if (Bmi < 30.0) return "Overweight";
                return "Obese";
            }
        }
    }
}