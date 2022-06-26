using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using U2.SharpTracker.Core;
using U2.SharpTracker.Web.Data;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class IndexModel : PageModel
    {
        private readonly ISharpTrackerService _trackerService;
        private readonly U2SharpTrackerWebContext _context;

        public IndexModel(ISharpTrackerService trackerService)
        {
            _trackerService = trackerService;
        }

        public IList<TrackerBranch> TrackerBranch { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var branches = await _trackerService.GetBranchesAsync(Guid.Empty, CancellationToken.None);
            TrackerBranch = branches.Select(b => new TrackerBranch(b)).ToList();
        }
    }
}
