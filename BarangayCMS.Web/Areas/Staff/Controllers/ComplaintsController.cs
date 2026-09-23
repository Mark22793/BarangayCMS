using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
// 🔑 INAYOS: Tamang namespace ng ViewModels
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

            var viewModelList = dtoList.Select(c => new ComplaintViewModel
            {
                Id = c.Id,
                ResidentId = c.ComplainantResidentId ?? 0,
                ResidentFullName = string.IsNullOrEmpty(c.ComplainantName) ? "Walk-in Resident" : c.ComplainantName,
                Subject = string.IsNullOrEmpty(c.CaseNumber) ? $"CMP-{c.Id}" : c.CaseNumber,
                Description = c.Details,
                DateSubmitted = c.CreatedDate,
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
                Id = c.Id,
                ResidentId = c.ComplainantResidentId ?? 0,
                ResidentFullName = string.IsNullOrEmpty(c.ComplainantName) ? "Walk-in Resident" : c.ComplainantName,
                Subject = string.IsNullOrEmpty(c.CaseNumber) ? $"CMP-{c.Id}" : c.CaseNumber,
                Description = c.Details,
                DateSubmitted = c.CreatedDate,
                Status = c.Status ?? "Pending"
            };

            return View(viewModel);
        }

        // GET: /Staff/Complaints/Create
        public async Task<IActionResult> Create()
        {
            var model = new ComplaintViewModel
            {
                DateSubmitted = DateTime.Now,
                Status = "Pending"
            };

            await PopulateResidentsDropDownList(model);
            return View(model);
        }

        // POST: /Staff/Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            if (ModelState.IsValid)
            {
                string complainantName = model.ResidentFullName ?? "";
                if (model.ResidentId > 0)
                {
                    var residents = await _residentService.GetAllResidentsAsync();
                    var resident = residents.FirstOrDefault(r => r.Id == model.ResidentId);
                    if (resident != null)
                    {
                        complainantName = $"{resident.LastName}, {resident.FirstName}";
                    }
                }

                var dto = new ComplaintDTO
                {
                    CaseNumber = string.IsNullOrEmpty(model.Subject)
                        ? $"BLOTTER-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}"
                        : model.Subject,
                    ComplainantResidentId = model.ResidentId > 0 ? model.ResidentId : null,
                    ComplainantName = complainantName,
                    Details = model.Description ?? string.Empty,
                    IncidentDate = model.DateSubmitted != default ? model.DateSubmitted : DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Status = model.Status ?? "Pending"
                };

                bool isSaved = await _complaintService.FileComplaintAsync(dto);
                if (isSaved) return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Hindi mai-save ang reklamo sa database.");
            }

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
                Id = c.Id,
                ResidentId = c.ComplainantResidentId ?? 0,
                ResidentFullName = c.ComplainantName,
                Subject = c.CaseNumber,
                Description = c.Details,
                DateSubmitted = c.IncidentDate != default ? c.IncidentDate : c.CreatedDate,
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
            ModelState.Remove("ResidentFullName");

            if (ModelState.IsValid)
            {
                bool isUpdated = await _complaintService.UpdateComplaintStatusAsync(model.Id, model.Status, model.Description ?? "Updated by Staff");
                if (isUpdated) return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, "Failed to update complaint record.");
            }

            await PopulateResidentsDropDownList(model);
            return View(model);
        }

        // GET: /Staff/Complaints/Assign/5
        public async Task<IActionResult> Assign(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = c.Id,
                ResidentFullName = c.ComplainantName,
                Subject = c.CaseNumber,
                Status = c.Status
            };

            return View(viewModel);
        }

        // POST: /Staff/Complaints/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, string designation)
        {
            bool isUpdated = await _complaintService.UpdateComplaintStatusAsync(id, "Assigned", $"Dispatched to {designation}");
            if (isUpdated) return RedirectToAction(nameof(Index));

            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/Complaints/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = c.Id,
                Subject = c.CaseNumber,
                Status = c.Status
            };

            return View(viewModel);
        }

        // POST: /Staff/Complaints/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(ComplaintViewModel model)
        {
            bool isUpdated = await _complaintService.UpdateComplaintStatusAsync(model.Id, model.Status, "Status transitioned by Staff");
            if (isUpdated) return RedirectToAction(nameof(Details), new { id = model.Id });

            return View(model);
        }

        // GET: /Staff/Complaints/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _complaintService.GetComplaintByIdAsync(id);
            if (c == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = c.Id,
                ResidentFullName = c.ComplainantName,
                Subject = c.CaseNumber,
                Status = c.Status
            };

            return View(viewModel);
        }

        // POST: /Staff/Complaints/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ComplaintViewModel model)
        {
            await _complaintService.UpdateComplaintStatusAsync(model.Id, "Dismissed", "Record Expunged / Dismissed by Staff");
            return RedirectToAction(nameof(Index));
        }

        // Helper Method
        private async Task PopulateResidentsDropDownList(ComplaintViewModel model)
        {
            var residents = await _residentService.GetAllResidentsAsync();
            var selectList = residents.Select(r => new {
                Id = r.Id,
                FullName = $"{r.LastName}, {r.FirstName} {r.MiddleName}".Trim()
            });

            ViewBag.ResidentsList = new SelectList(selectList, "Id", "FullName", model.ResidentId);
        }
    }
}