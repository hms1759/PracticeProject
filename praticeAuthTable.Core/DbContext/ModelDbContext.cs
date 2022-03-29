using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace practiceAuthTable.Core.Models
{
   public class ModelDbContext : DbContext
    {
        public ModelDbContext(DbContextOptions<ModelDbContext> options) : base(options)
        {

        }
        //  public DbSet<CashAdvance> DbCashAdvance { get; set; }
        // public DbSet<staffs> DbDepartment { get; set; }
        public DbSet<Items> ItemsTable { get; set; }
        public DbSet<Items2> ItemsTable2 { get; set; }

    }
    
}
