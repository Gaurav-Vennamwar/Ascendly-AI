using Ascendly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ascendly.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        public DbSet<InterviewSession> InterviewSessions { get; set; }
        public DbSet<InterviewTopic> InterviewTopics { get; set; }
        public DbSet<InterviewQuestion> InterviewQuestions { get; set; }
        public DbSet<InterviewAnswer> InterviewAnswers { get; set; }
        public DbSet<QuestionEvaluation> QuestionEvaluations { get; set; }
        public DbSet<FinalInterviewEvaluation> FinalInterviewEvaluations { get; set; }

        //I used OnModelCreating to configure entity relationships with Fluent API. It keeps the model configuration centralized and is useful for more complex mappings

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Refresh token relationship
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            //created a one-to-many relationship because a user may request email verification multiple times if previous links expire


            // Email verification relationship
            modelBuilder.Entity<EmailVerificationToken>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> InterviewSession
            modelBuilder.Entity<InterviewSession>()
                .HasOne(x => x.User)
                .WithMany(u => u.InterviewSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // InterviewSession -> InterviewTopic
            modelBuilder.Entity<InterviewTopic>()
                .HasOne(x => x.InterviewSession)
                .WithMany(x => x.Topics)
                .HasForeignKey(x => x.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // InterviewTopic -> InterviewQuestion
            modelBuilder.Entity<InterviewQuestion>()
                .HasOne(x => x.InterviewTopic)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.InterviewTopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // InterviewQuestion -> InterviewAnswer
            modelBuilder.Entity<InterviewAnswer>()
                .HasOne(x => x.InterviewQuestion)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.InterviewQuestionId)
                .OnDelete(DeleteBehavior.Cascade);



            // TopicEvaluation -> QuestionEvaluation
            // InterviewQuestion -> QuestionEvaluation
                modelBuilder.Entity<QuestionEvaluation>()
                .HasOne(x => x.InterviewQuestion)
                .WithMany(x => x.Evaluations)
                .HasForeignKey(x => x.InterviewQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // InterviewQuestion -> QuestionEvaluation
            modelBuilder.Entity<QuestionEvaluation>()
             .HasOne(x => x.InterviewQuestion)
             .WithMany(x => x.Evaluations)
             .HasForeignKey(x => x.InterviewQuestionId)
             .OnDelete(DeleteBehavior.Cascade);

            // InterviewSession -> FinalInterviewEvaluation
            modelBuilder.Entity<FinalInterviewEvaluation>()
                .HasOne(x => x.InterviewSession)
                .WithMany()
                .HasForeignKey(x => x.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostgreSQL arrays for evaluation lists
            modelBuilder.Entity<QuestionEvaluation>()
                .Property(x => x.Strengths)
                .HasColumnType("text[]");

            modelBuilder.Entity<QuestionEvaluation>()
                .Property(x => x.Mistakes)
                .HasColumnType("text[]");

            modelBuilder.Entity<QuestionEvaluation>()
                .Property(x => x.Improvements)
                .HasColumnType("text[]");

            modelBuilder.Entity<FinalInterviewEvaluation>()
                .Property(x => x.StrongestAreas)
                .HasColumnType("text[]");

            modelBuilder.Entity<FinalInterviewEvaluation>()
                .Property(x => x.AreasToImprove)
                .HasColumnType("text[]");

            modelBuilder.Entity<FinalInterviewEvaluation>()
                .Property(x => x.RepeatedMistakes)
                .HasColumnType("text[]");

            modelBuilder.Entity<FinalInterviewEvaluation>()
                .Property(x => x.PreparationPlan)
                .HasColumnType("text[]");
        }
    }
}
