using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace Web
{
     public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           
            builder.Entity<AgentCustomer>().HasKey(sc => new { sc.CustomerId, sc.AgentId });
            builder.Entity<InvoicePallet>().HasKey(sc => new { sc.InvoiceId, sc.PalletId });
            builder.Entity<InvoiceBox>().HasKey(sc => new { sc.InvoiceId, sc.BoxId });
            builder.Entity<InvoiceDocumentFile>().HasKey(sc => new { sc.InvoiceId, sc.DocumentFileId });
            builder.Entity<PalletDataView>().HasKey(sc => new { sc.PalletId });
            builder.Entity<PalletFilter>().HasKey(sc => new { sc.PalletFilterId }); 
            builder.Entity<Pallet>().HasKey(sc=>sc.PalletId)         ;
        }
      
        public DbSet<PalletDataView> PalletDataView{get;set;}
        public DbSet<PalletFilter> PalletFilter{get;set;}
        public DbSet<Pallet> Pallet{get;set;}
        
    }
}
