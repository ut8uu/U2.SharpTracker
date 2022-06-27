using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using U2.SharpTracker.Core;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class CreateModel : PageModel
    {
        private readonly ISharpTrackerService _trackerService;

        public CreateModel(ISharpTrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public TrackerBranch TrackerBranch { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var branch = new BranchDto
            {
                Id = Guid.NewGuid(),
                LoadStatusCode = UrlLoadStatusCode.Unknown,
                LoadState = UrlLoadState.Unknown,
                Url = TrackerBranch.Url,
                Title = TrackerBranch.Title,
                OriginalId = TrackerBranch.OriginalId,
                ParentId = Guid.Empty,
            };
            await _trackerService.AddOrUpdateBranchAsync(branch, CancellationToken.None);
            return RedirectToPage("./Index");
        }
    }
}
