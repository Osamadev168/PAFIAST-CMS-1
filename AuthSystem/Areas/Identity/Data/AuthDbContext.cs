using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{


    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {

    }
    public DbSet<MCQ> MCQs { get; set; }
    public DbSet<Blank> Blanks { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<TestDetail> TestsDetail { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<TestSession> TestSessions { get; set; }
    public DbSet<AssignedQuestions> AssignedQuestions { get; set; }
    public DbSet<UserTestSession> UserTestSessions { get; set; }
    public DbSet<TestCalenders> TestCalenders { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AssignedQuestions>()
        .HasOne(aq => aq.TestDetail)
        .WithMany()
        .HasForeignKey(aq => aq.TestDetailId)
        .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<AssignedQuestions>()
            .HasOne(aq => aq.Question)
            .WithMany()
            .HasForeignKey(aq => aq.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
     
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
