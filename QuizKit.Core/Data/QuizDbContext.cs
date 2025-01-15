using Microsoft.EntityFrameworkCore;
using QuizKit.Core.Entities;

namespace QuizKit.Core.Data;

public class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserProfileConfig).Assembly);
    }
    
    public required DbSet<Category> Categories { get; set; }
    public required DbSet<Invitation> Invitations { get; set; }
    public required DbSet<Organization> Organizations { get; set; }
    public required DbSet<PracticeQuestion> PracticeQuestions { get; set; }
    public required DbSet<Quiz> Quizzes { get; set; }
    public required DbSet<QuizQuestion> QuizQuestions { get; set; }
    public required DbSet<QuizResult> QuizResults { get; set; }
    public required DbSet<UserOrganization> UserOrganizations { get; set; }
    public required DbSet<UserProfile> Users { get; set; }
    public required DbSet<UserRole> UserRoles { get; set; }    
}
