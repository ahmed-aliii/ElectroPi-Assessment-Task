using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain;

namespace TMS.Infrastructure
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.OwnerId)
                .HasMaxLength(450)
                .IsRequired();

            builder.HasIndex(p => p.OwnerId);

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.IsDeleted).HasDefaultValue(false);

            builder.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(p => p.Tasks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
