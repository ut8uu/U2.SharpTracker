using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class EditModel : PageModel
    {
        public EditModel()
        {
        }

        [BindProperty]
        public TrackerBranch TrackerBranch { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            /*
            if (id == null || _context.TrackerBranch == null)
            {
                return NotFound();
            }

            var trackerbranch =  await _context.TrackerBranch.FirstOrDefaultAsync(m => m.Id == id);
            if (trackerbranch == null)
            {
                return NotFound();
            }
            TrackerBranch = trackerbranch;
            */
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            /*
            _context.Attach(TrackerBranch).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrackerBranchExists(TrackerBranch.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            */

            return RedirectToPage("./Index");
        }

        private bool TrackerBranchExists(Guid id)
        {
            return true;
//          return (_context.TrackerBranch?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
