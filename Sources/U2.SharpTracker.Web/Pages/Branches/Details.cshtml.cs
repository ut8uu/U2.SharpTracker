using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class DetailsModel : PageModel
    {
        public DetailsModel()
        {
        }

      public TrackerBranch TrackerBranch { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            /*
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
            */
            return Page();
        }
    }
}
