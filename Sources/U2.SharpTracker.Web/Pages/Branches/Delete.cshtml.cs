using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using U2.SharpTracker.Web.Data;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class DeleteModel : PageModel
    {
        private readonly U2.SharpTracker.Web.Data.U2SharpTrackerWebContext _context;

        public DeleteModel(U2.SharpTracker.Web.Data.U2SharpTrackerWebContext context)
        {
            _context = context;
        }

        [BindProperty]
      public TrackerBranch TrackerBranch { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.TrackerBranch == null)
            {
                return NotFound();
            }

            var trackerbranch = await _context.TrackerBranch.FirstOrDefaultAsync(m => m.Id == id);

            if (trackerbranch == null)
            {
                return NotFound();
            }
            else 
            {
                TrackerBranch = trackerbranch;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null || _context.TrackerBranch == null)
            {
                return NotFound();
            }
            var trackerbranch = await _context.TrackerBranch.FindAsync(id);

            if (trackerbranch != null)
            {
                TrackerBranch = trackerbranch;
                _context.TrackerBranch.Remove(TrackerBranch);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
