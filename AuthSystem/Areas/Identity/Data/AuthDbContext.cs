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
    public DbSet<TestApplication> TestApplications { get; set; }
    public DbSet<TestCenters> TestCenters { get; set; }
    public DbSet<FeeVoucher> FeeVoucher { get; set; }
    public DbSet<AdmitCard> AdmitCards { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<CenterChangeRequest> CenterChangeRequests { get; set; }
    public DbSet<UserRolesViewModel> UserRolesViewModels { get; set; }
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
        builder.Entity<TestApplication>()
                .HasOne(uc => uc.Test)
                .WithMany()
                .HasForeignKey(uc => uc.TestId)
                .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<TestApplication>()
            .HasOne(uc => uc.Calendar)
            .WithMany()
            .HasForeignKey(uc => uc.CalendarId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<UserRolesViewModel>()
           .HasNoKey();
        base.OnModelCreating(builder);

        
    }
}
