using Microsoft.EntityFrameworkCore;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileConfig).Assembly);
    }
    
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<PracticeQuestion> PracticeQuestions { get; set; } = null!;
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
    public DbSet<QuizResult> QuizResults { get; set; } = null!;
    public DbSet<UserOrganization> UserOrganizations { get; set; } = null!;
    public DbSet<UserProfile> Users { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;    
}
