using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using BarangayCMS.Web.Areas.Staff.ViewModels;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ComplaintsController : Controller
    {
        private readonly IComplaintService _complaintService;
        private readonly IResidentService _residentService;

        public ComplaintsController(IComplaintService complaintService, IResidentService residentService)
        {
            _complaintService = complaintService;
            _residentService = residentService;
        }

        // GET: /Staff/Complaints
        public async Task<IActionResult> Index()
        {
            var dtoList = await _complaintService.GetAllComplaintsAsync();

            // 🔍 FILTRATION: Itago ang mga record na "Dismissed" o "Deleted" na para mawala sa listahan
            var activeComplaints = dtoList.Where(c => c.Status != "Dismissed" && c.Status != "Deleted");

            var viewModelList = activeComplaints.Select(c => new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentId = c.ComplainantResidentId,
                ComplainantName = string.IsNullOrEmpty(c.ComplainantName) ? "Walk-in Resident" : c.ComplainantName,
                Subject = string.IsNullOrEmpty(c.CaseNumber) ? $"CMP-{c.Id}" : c.CaseNumber,
                Description = c.Details,
                IncidentDate = c.CreatedDate,
                Status = c.Status ?? "Pending"
            }).ToList();

            return View(viewModelList);
        }

        // GET: /Staff/Complaints/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentId = c.ComplainantResidentId,
                ComplainantName = string.IsNullOrEmpty(c.ComplainantName) ? "Walk-in Resident" : c.ComplainantName,
                Subject = string.IsNullOrEmpty(c.CaseNumber) ? $"CMP-{c.Id}" : c.CaseNumber,
                Description = c.Details,
                IncidentDate = c.CreatedDate,
                Status = c.Status ?? "Pending"
            };

            return View(viewModel);
        }

        // GET: /Staff/Complaints/Create
        public async Task<IActionResult> Create()
        {
            var model = new ComplaintViewModel
            {
                IncidentDate = DateTime.Now,
                Status = "Pending Audit"
            };

            await PopulateResidentsDropDownList(model);
            return View(model);
        }

        // POST: /Staff/Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            ModelState.ClearValidationState(nameof(ComplaintViewModel));

            string complainantName = "Walk-in Resident";
            if (model.ResidentId.HasValue && model.ResidentId.Value > 0)
            {
                var residents = await _residentService.GetAllResidentsAsync();
                var resident = residents.FirstOrDefault(r => r.Id == model.ResidentId.Value);
                if (resident != null)
                {
                    complainantName = $"{resident.LastName}, {resident.FirstName}";
                }
            }

            var dto = new ComplaintDTO
            {
                CaseNumber = string.IsNullOrWhiteSpace(model.Subject)
                    ? $"BLOTTER-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}"
                    : model.Subject,
                ComplainantResidentId = (model.ResidentId.HasValue && model.ResidentId.Value > 0) ? model.ResidentId : null,
                ComplainantName = complainantName,
                Details = model.Description ?? string.Empty,
                IncidentDate = model.IncidentDate != default ? model.IncidentDate : DateTime.Now,
                CreatedDate = DateTime.Now,
                Status = model.Status ?? "Pending Audit"
            };

            bool isSaved = await _complaintService.FileComplaintAsync(dto);
            if (isSaved) return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Hindi mai-save ang reklamo sa database.");
            await PopulateResidentsDropDownList(model);
            return View(model);
        }

        // GET: /Staff/Complaints/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                ComplaintId = c.Id,
                ResidentId = c.ComplainantResidentId,
                ComplainantName = c.ComplainantName,
                Subject = c.CaseNumber,
                Description = c.Details,
                IncidentDate = c.IncidentDate != default ? c.IncidentDate : c.CreatedDate,
                Status = c.Status
            };

            await PopulateResidentsDropDownList(viewModel);
            return View(viewModel);
        }

        // POST: /Staff/Complaints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ComplaintViewModel model)
        {
            ModelState.ClearValidationState(nameof(ComplaintViewModel));

            bool isUpdated = await _complaintService.UpdateComplaintStatusAsync(
                model.Id,
                model.Status ?? "Pending",
                model.Description ?? "Updated by Staff"
            );

            if (isUpdated) return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Failed to update complaint record.");
            await PopulateResidentsDropDownList(model);
            return View(model);
        }

        // POST: /Staff/Complaints/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ComplaintViewModel model)
        {
            int targetId = model.Id > 0 ? model.Id : model.ComplaintId;

            if (targetId > 0)
            {
                // Baguhin ang status sa "Dismissed" para ma-filter out sa Index list
                await _complaintService.UpdateComplaintStatusAsync(targetId, "Dismissed", "Record Expunged / Deleted by Staff");
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper Method
        private async Task PopulateResidentsDropDownList(ComplaintViewModel model)
        {
            var residents = await _residentService.GetAllResidentsAsync() ?? new System.Collections.Generic.List<ResidentDTO>();

            var selectList = residents.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(r.MiddleName)
                    ? $"{r.LastName}, {r.FirstName}"
                    : $"{r.LastName}, {r.FirstName} {r.MiddleName}",
                Selected = model.ResidentId.HasValue && r.Id == model.ResidentId.Value
            }).OrderBy(r => r.Text).ToList();

            ViewBag.ResidentsList = new SelectList(selectList, "Value", "Text", model.ResidentId);
        }
    }
}