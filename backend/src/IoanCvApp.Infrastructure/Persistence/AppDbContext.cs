using System.Text.Json;
using IoanCvApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoanCvApp.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ExperienceEntry> ExperienceEntries => Set<ExperienceEntry>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Headline).IsRequired().HasMaxLength(300);
            b.Property(x => x.Bio).IsRequired();
            b.Property(x => x.Skills).AsJsonStringList();
        });

        modelBuilder.Entity<ExperienceEntry>(b =>
        {
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Company).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).IsRequired();
            b.Property(x => x.TechTags).AsJsonStringList();
        });

        modelBuilder.Entity<Project>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).IsRequired();
            b.Property(x => x.TechStack).AsJsonStringList();
        });
    }
}

internal static class JsonListExtensions
{
    public static PropertyBuilder<List<string>> AsJsonStringList(this PropertyBuilder<List<string>> builder)
    {
        builder.HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
            new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList()));
        builder.HasColumnType("TEXT");
        return builder;
    }
}
