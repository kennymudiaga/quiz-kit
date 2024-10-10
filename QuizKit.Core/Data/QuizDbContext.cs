using Microsoft.EntityFrameworkCore;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<PracticeQuestion> PracticeQuestions { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizResult> QuizResults { get; set; }
    public DbSet<UserOrganization> UserOrganizations { get; set; }
    public DbSet<UserProfile> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileConfig).Assembly);
    }
}
