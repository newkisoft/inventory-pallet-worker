using Microsoft.EntityFrameworkCore;
using newkilibraries;

namespace newki_inventory_pallet
{
    public class ApplicationDbContext : DbContext
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
            builder.Entity<PalletFilter>().HasKey(sc => new { sc.PalletFilterId });            
        }
        public DbSet<Pallet> Pallet { get; set; }
        public DbSet<PalletFilter> PalletFilter{get;set;}
    }
}