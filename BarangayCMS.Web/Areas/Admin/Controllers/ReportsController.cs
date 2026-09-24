using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.DAL.Context;
using BarangayCMS.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,BarangayStaff")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. MAIN DASHBOARD WITH ANALYTICS & CHARTS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var currentMonth = today.Month;

            var yearlyComplaints = await _context.Complaints
                .Where(c => c.DateSubmitted.Year == currentYear)
                .GroupBy(c => c.DateSubmitted.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            var yearlyCertificates = await _context.Certificates
                .Where(c => (c.DateIssued.HasValue && c.DateIssued.Value.Year == currentYear)
                         || (c.DateRequested.Year == currentYear))
                .GroupBy(c => c.DateIssued.HasValue ? c.DateIssued.Value.Month : c.DateRequested.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            var yearlyResidents = await _context.Residents
                .Where(r => r.CreatedAt.Year == currentYear)
                .GroupBy(r => r.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            var monthsList = new List<string>();
            var complaintsData = new List<int>();
            var certificatesData = new List<int>();
            var residentsData = new List<int>();

            for (int m = 1; m <= currentMonth; m++)
            {
                monthsList.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m));
                complaintsData.Add(yearlyComplaints.GetValueOrDefault(m, 0));
                certificatesData.Add(yearlyCertificates.GetValueOrDefault(m, 0));
                residentsData.Add(yearlyResidents.GetValueOrDefault(m, 0));
            }

            var lastMonthDate = today.AddMonths(-1);
            int complaintsThisMonth = complaintsData.LastOrDefault();
            int complaintsLastMonth = await _context.Complaints
                .CountAsync(c => c.DateSubmitted.Month == lastMonthDate.Month && c.DateSubmitted.Year == lastMonthDate.Year);

            string complaintTrend;
            if (complaintsLastMonth == 0)
            {
                complaintTrend = complaintsThisMonth > 0 ? $"Tumaas ng {complaintsThisMonth} kaso" : "Walang kaso ngayong buwan";
            }
            else
            {
                int diff = complaintsThisMonth - complaintsLastMonth;
                if (diff > 0)
                    complaintTrend = $"Tumaas ng {diff} kaso (+{((double)diff / complaintsLastMonth * 100):F0}%)";
                else if (diff < 0)
                    complaintTrend = $"Bumaba ng {Math.Abs(diff)} kaso (-{((double)Math.Abs(diff) / complaintsLastMonth * 100):F0}%)";
                else
                    complaintTrend = "Walang pagbabago mula nakaraang buwan";
            }

            int newResidentsThisMonth = residentsData.LastOrDefault();

            // DEMOGRAPHIC ANALYTICS
            var demographics = await _context.Residents
                .Where(r => r.IsResident)
                .Select(r => new { r.BirthDate, r.IsPwd })
                .ToListAsync();

            int ComputeAge(DateTime birth)
            {
                var age = today.Year - birth.Year;
                if (birth.Date > today.AddYears(-age)) age--;
                return age < 0 ? 0 : age;
            }

            int childCount = 0, youthCount = 0, adultCount = 0, seniorCount = 0, pwdCount = 0;
            foreach (var d in demographics)
            {
                int age = ComputeAge(d.BirthDate);
                if (age < 15) childCount++;
                else if (age <= 30) youthCount++;
                else if (age <= 59) adultCount++;
                else seniorCount++;

                if (d.IsPwd) pwdCount++;
            }

            var dashboardData = new ReportsDashboardViewModel
            {
                TotalRegisteredPopulation = demographics.Count,
                YouthPopulation = youthCount,
                SeniorCitizenPopulation = seniorCount,
                PwdPopulation = pwdCount,
                AgeGroupLabels = new List<string> { "Child (0-14)", "Youth (15-30)", "Adult (31-59)", "Senior (60+)" },
                AgeGroupCounts = new List<int> { childCount, youthCount, adultCount, seniorCount },

                TotalResidents = await _context.Residents.CountAsync(r => r.IsResident),
                TotalComplaints = await _context.Complaints.CountAsync(),
                TotalBudget = await _context.Budgets.SumAsync(b => (decimal?)b.TotalAllocation) ?? 0m,
                ActiveProjects = await _context.Projects.CountAsync(p => p.Status != "Completed"),
                TotalCertificatesIssued = await _context.Certificates.CountAsync(c => c.Status == "Issued" || c.Status == "Approved"),
                ActiveEvacuees = await _context.Disasters.SumAsync(d => (int?)d.DisplacedIndividualsCount) ?? 0,
                TotalHealthRecords = await _context.HealthRecords.CountAsync(),

                CertificatesIssuedThisMonth = certificatesData.LastOrDefault(),
                ComplaintsThisMonth = complaintsThisMonth,
                ComplaintsLastMonth = complaintsLastMonth,
                ComplaintTrendStatus = complaintTrend,
                NewResidentsThisMonth = newResidentsThisMonth,
                DeceasedOrMovedThisMonth = await _context.Residents.CountAsync(r => !r.IsResident),
                ResidentTrendStatus = newResidentsThisMonth > 0 ? $"+{newResidentsThisMonth} bagong rehistro ngayong buwan" : "Walang bagong rehistro",

                Months = monthsList,
                MonthlyComplaints = complaintsData,
                MonthlyCertificates = certificatesData,
                MonthlyResidents = residentsData
            };

            return View(dashboardData);
        }

        // ==========================================
        // 2. SUB-MODULE REPORTS
        // ==========================================
        public async Task<IActionResult> Residents()
        {
            var data = await _context.Residents
                .AsNoTracking()
                .Select(r => new ResidentReportItem
                {
                    Id = r.ResidentId,
                    FullName = (r.FirstName + " " + (string.IsNullOrEmpty(r.MiddleName) ? "" : r.MiddleName + " ") + r.LastName + " " + r.Suffix).Trim(),
                    Age = DateTime.Now.Year - r.BirthDate.Year - (DateTime.Now.DayOfYear < r.BirthDate.DayOfYear ? 1 : 0),
                    Gender = r.Gender,
                    CivilStatus = r.CivilStatus,
                    Address = (r.HouseNumber + " " + r.Street + " " + r.SitioPurok).Trim(),
                    ContactNumber = r.ContactNumber,
                    VoterStatus = r.IsVoter ? "Registered Voter" : "Non-Voter",
                    IsResident = r.IsResident,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Complaints()
        {
            var data = await _context.Complaints
                .AsNoTracking()
                .Select(c => new ComplaintReportItem
                {
                    Id = c.ComplaintId,
                    CaseNumber = c.CaseNumber,
                    ComplainantName = c.ComplainantName,
                    RespondentName = c.RespondentName,
                    IncidentLocation = c.IncidentLocation,
                    IncidentDate = c.IncidentDate,
                    Status = c.Status,
                    DateSubmitted = c.DateSubmitted
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Certificates()
        {
            var data = await _context.Certificates
                .AsNoTracking()
                .Select(c => new CertificateReportItem
                {
                    Id = c.CertificateId,
                    ControlNumber = c.ControlNumber,
                    ResidentName = c.ResidentName,
                    CertificateType = c.CertificateType,
                    Purpose = c.Purpose,
                    Status = c.Status,
                    DateRequested = c.DateRequested,
                    DateIssued = c.DateIssued
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Budget()
        {
            var data = await _context.Budgets
                .AsNoTracking()
                .Select(b => new BudgetReportItem
                {
                    Id = b.BudgetId,
                    Year = b.Year,
                    Category = b.Category,
                    Description = b.Description,
                    TotalAllocation = b.TotalAllocation,
                    DisbursedAmount = b.DisbursedAmount,
                    LoggedBy = b.LoggedBy
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Projects()
        {
            var data = await _context.Projects
                .AsNoTracking()
                .Select(p => new ProjectReportItem
                {
                    Id = p.ProjectId,
                    Title = p.Title,
                    Location = p.Location,
                    BudgetAllocated = p.BudgetAllocated,
                    TotalExpenses = p.TotalExpenses,
                    Contractor = p.Contractor,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Disaster()
        {
            var data = await _context.Disasters
                .AsNoTracking()
                .Select(d => new DisasterReportItem
                {
                    Id = d.DisasterId,
                    IncidentName = d.IncidentName,
                    DisasterType = d.DisasterType,
                    OccurrenceDate = d.OccurrenceDate,
                    AffectedHouseholdsCount = d.AffectedHouseholdsCount,
                    DisplacedIndividualsCount = d.DisplacedIndividualsCount,
                    CasualtiesCount = d.CasualtiesCount,
                    EvacuationCenterStatus = d.EvacuationCenterStatus,
                    ReliefDistributionStatus = d.ReliefDistributionStatus
                })
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Health()
        {
            var data = await _context.HealthRecords
                .AsNoTracking()
                .Include(h => h.Resident)
                .Select(h => new HealthReportItem
                {
                    Id = h.HealthRecordId,
                    ResidentName = h.Resident != null ? (h.Resident.FirstName + " " + h.Resident.LastName) : "N/A",
                    WeightKg = h.WeightKg,
                    HeightCm = h.HeightCm,
                    BloodType = h.BloodType,
                    IsVaccinated = h.IsVaccinated,
                    MedicalCondition = h.MedicalCondition,
                    HealthClassification = h.HealthClassification,
                    LastCheckupDate = h.LastCheckupDate,
                    AttendingHealthWorker = h.AttendingHealthWorker
                })
                .ToListAsync();

            return View(data);
        }
    }
}