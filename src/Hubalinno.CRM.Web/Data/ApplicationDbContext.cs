using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<AccountBusinessLine> AccountBusinessLines => Set<AccountBusinessLine>();
    public DbSet<InvestorProfile> InvestorProfiles => Set<InvestorProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Account>()
            .HasMany(a => a.Contacts)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Account>()
            .HasOne(a => a.InvestorProfile)
            .WithOne(i => i.Account)
            .HasForeignKey<InvestorProfile>(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvestorProfile>()
            .Property(i => i.TypicalTicketMin)
            .HasPrecision(18, 2);

        builder.Entity<InvestorProfile>()
            .Property(i => i.TypicalTicketMax)
            .HasPrecision(18, 2);

        builder.Entity<Product>()
            .HasIndex(p => p.Key)
            .IsUnique();

        builder.Entity<Subscription>()
            .HasOne(s => s.Account)
            .WithMany()
            .HasForeignKey(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Subscription>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(o => o.Account)
            .WithMany()
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(o => o.Contact)
            .WithMany()
            .HasForeignKey(o => o.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .HasOne(o => o.Product)
            .WithMany()
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .Property(o => o.EstimatedValue)
            .HasPrecision(18, 2);

        builder.Entity<Opportunity>()
            .HasOne(o => o.PipelineStage)
            .WithMany()
            .HasForeignKey(o => o.PipelineStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Opportunity>()
            .HasOne(o => o.NextActionActivity)
            .WithMany()
            .HasForeignKey(o => o.NextActionActivityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Opportunity>()
            .Property(o => o.NextActionSubject)
            .HasMaxLength(200);

        builder.Entity<Opportunity>()
            .HasIndex(o => o.NextActionDueDate);

        builder.Entity<Activity>()
            .HasOne(a => a.Account)
            .WithMany()
            .HasForeignKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(a => a.Opportunity)
            .WithMany()
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(a => a.Contact)
            .WithMany()
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Activity>()
            .HasOne(a => a.AssignedToUser)
            .WithMany()
            .HasForeignKey(a => a.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasIndex(a => new { a.BusinessLine, a.Status, a.DueDate });

        builder.Entity<PipelineStage>()
            .HasIndex(p => new { p.BusinessLine, p.Key })
            .IsUnique();

        builder.Entity<PipelineStage>()
            .Property(p => p.Key)
            .HasMaxLength(50);

        builder.Entity<PipelineStage>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        builder.Entity<AccountBusinessLine>()
            .HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AccountBusinessLine>()
            .HasIndex(l => new { l.AccountId, l.BusinessLine })
            .IsUnique();
    }
}
