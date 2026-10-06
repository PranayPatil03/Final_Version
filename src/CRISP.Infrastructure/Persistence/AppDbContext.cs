using Microsoft.EntityFrameworkCore;
using CRISP.Domain.Entities;

namespace CRISP.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomerMaster> CustomerMasters => Set<CustomerMaster>();
    public DbSet<IndustryDetails> IndustryDetails => Set<IndustryDetails>();
    public DbSet<BusinessParameterDetails> BusinessParameterDetails => Set<BusinessParameterDetails>();
    public DbSet<WorkflowMaster> WorkflowMasters => Set<WorkflowMaster>();
    public DbSet<WorkflowJsonVersion> WorkflowJsonVersions => Set<WorkflowJsonVersion>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<ParameterDefinition> ParameterDefinitions => Set<ParameterDefinition>();
    public DbSet<ScoreDefinition> ScoreDefinitions => Set<ScoreDefinition>();
    public DbSet<RuleDefinition> RuleDefinitions => Set<RuleDefinition>();
    public DbSet<FrameworkInstance> FrameworkInstances => Set<FrameworkInstance>();
    public DbSet<FrameworkParameterValue> FrameworkParameterValues => Set<FrameworkParameterValue>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<CustomerMaster>().ToTable("CustomerMaster").HasKey(x => x.Id);
        b.Entity<CustomerMaster>().HasIndex(x => x.EntityName).IsUnique();
        b.Entity<CustomerMaster>().Property(x => x.EntityName).HasMaxLength(250).IsRequired();

        b.Entity<IndustryDetails>().ToTable("IndustryDetails").HasKey(x => x.Id);
        b.Entity<IndustryDetails>().HasIndex(x => new { x.WorkflowMasterId, x.IndustryName }).IsUnique();
        b.Entity<IndustryDetails>().Property(x => x.IndustryName).HasMaxLength(250).IsRequired();
        b.Entity<IndustryDetails>().HasOne(x => x.WorkflowMaster).WithMany().HasForeignKey(x => x.WorkflowMasterId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<BusinessParameterDetails>().ToTable("BusinessParameterDetails").HasKey(x => x.Id);
        b.Entity<BusinessParameterDetails>().Property(x => x.Weight).HasPrecision(10, 2);
        b.Entity<BusinessParameterDetails>().HasOne(x => x.FrameworkInstance).WithMany(x => x.IndustrySegments).HasForeignKey(x => x.FrameworkInstanceId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<BusinessParameterDetails>().HasOne(x => x.IndustrySegment).WithMany(x => x.FrameworkSegments).HasForeignKey(x => x.IndustrySegmentId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<WorkflowMaster>().ToTable("WorkflowMaster").HasKey(x => x.Id);
        b.Entity<WorkflowMaster>().HasIndex(x => x.WorkflowCode).IsUnique();
        b.Entity<WorkflowMaster>().Property(x => x.WorkflowCode).HasMaxLength(100).IsRequired();
        b.Entity<WorkflowMaster>().Property(x => x.WorkflowName).HasMaxLength(200).IsRequired();

        b.Entity<WorkflowStep>().ToTable("WorkflowStep").HasKey(x => x.Id);
        b.Entity<WorkflowStep>().HasOne(x => x.WorkflowMaster).WithMany(x => x.Steps).HasForeignKey(x => x.WorkflowMasterId);
        b.Entity<WorkflowStep>().HasIndex(x => new { x.WorkflowMasterId, x.DisplayOrder }).IsUnique();
        b.Entity<WorkflowStep>().Property(x => x.StepName).HasMaxLength(150).IsRequired();

        b.Entity<WorkflowJsonVersion>().ToTable("WorkflowJsonVersion").HasKey(x => x.Id);
        b.Entity<WorkflowJsonVersion>().HasIndex(x => new { x.WorkflowMasterId, x.VersionNo }).IsUnique();
        b.Entity<WorkflowJsonVersion>().Property(x => x.JsonContent).HasColumnType("nvarchar(max)");
        b.Entity<WorkflowJsonVersion>().HasOne(x => x.WorkflowMaster).WithMany(x => x.Versions).HasForeignKey(x => x.WorkflowMasterId);

        b.Entity<ParameterDefinition>().ToTable("ParameterDefinition").HasKey(x => x.Id);
        b.Entity<ParameterDefinition>().HasIndex(x => new { x.WorkflowMasterId, x.ParameterCode }).IsUnique();
        b.Entity<ParameterDefinition>().Property(x => x.Weight).HasPrecision(10, 2);
        b.Entity<ParameterDefinition>().HasOne(x => x.WorkflowMaster).WithMany(x => x.Parameters).HasForeignKey(x => x.WorkflowMasterId);

        b.Entity<ScoreDefinition>().ToTable("ScoreDefinition").HasKey(x => x.Id);
        b.Entity<ScoreDefinition>().HasIndex(x => new { x.ParameterDefinitionId, x.Score }).IsUnique();
        b.Entity<ScoreDefinition>().Property(x => x.ThresholdValue).HasPrecision(18, 4);
        b.Entity<ScoreDefinition>().Property(x => x.MinValue).HasPrecision(18, 4);
        b.Entity<ScoreDefinition>().Property(x => x.MaxValue).HasPrecision(18, 4);
        b.Entity<ScoreDefinition>().HasOne(x => x.ParameterDefinition).WithMany(x => x.Scores).HasForeignKey(x => x.ParameterDefinitionId);

        b.Entity<RuleDefinition>().ToTable("RuleDefinition").HasKey(x => x.Id);
        b.Entity<RuleDefinition>().HasIndex(x => new { x.WorkflowMasterId, x.RuleName }).IsUnique();
        b.Entity<RuleDefinition>().Property(x => x.RuleName).HasMaxLength(200).IsRequired();
        b.Entity<RuleDefinition>().Property(x => x.SuccessEvent).HasMaxLength(200).IsRequired();
        b.Entity<RuleDefinition>().Property(x => x.RuleExpressionType).HasMaxLength(100).IsRequired();
        b.Entity<RuleDefinition>().Property(x => x.Expression).HasColumnType("nvarchar(max)");
        b.Entity<RuleDefinition>().Property(x => x.OutputExpression).HasColumnType("nvarchar(max)");
        b.Entity<RuleDefinition>().HasOne(x => x.WorkflowMaster).WithMany(x => x.Rules).HasForeignKey(x => x.WorkflowMasterId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<FrameworkInstance>().ToTable("FrameworkInstance").HasKey(x => x.Id);
        b.Entity<FrameworkInstance>().HasOne(x => x.WorkflowMaster).WithMany().HasForeignKey(x => x.WorkflowMasterId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FrameworkInstance>().HasOne(x => x.Entity).WithMany(x => x.Frameworks).HasForeignKey(x => x.EntityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<FrameworkParameterValue>().ToTable("FrameworkParameterValue").HasKey(x => x.Id);
        b.Entity<FrameworkParameterValue>().Property(x => x.Value).HasPrecision(18, 4);
        b.Entity<FrameworkParameterValue>().HasOne(x => x.FrameworkInstance).WithMany(x => x.ParameterValues).HasForeignKey(x => x.FrameworkInstanceId);
        b.Entity<FrameworkParameterValue>().HasOne(x => x.ParameterDefinition).WithMany().HasForeignKey(x => x.ParameterDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }
}
