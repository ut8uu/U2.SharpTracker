using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using U2.SharpTracker.Web.Data;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Pages.Branches
{
    public class CreateModel : PageModel
    {
        private readonly U2SharpTrackerWebContext _context;

        public CreateModel()//U2SharpTrackerWebContext context)
        {
            //_context = context;
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
          if (!ModelState.IsValid || _context.TrackerBranch == null || TrackerBranch == null)
            {
                return Page();
            }

            _context.TrackerBranch.Add(TrackerBranch);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
