using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using U2.SharpTracker.Web.Models;

namespace U2.SharpTracker.Web.Data
{
    public class U2SharpTrackerWebContext : DbContext
    {
        public U2SharpTrackerWebContext (DbContextOptions<U2SharpTrackerWebContext> options)
            : base(options)
        {
        }

        public DbSet<U2.SharpTracker.Web.Models.TrackerBranch>? TrackerBranch { get; set; }
    }
}
