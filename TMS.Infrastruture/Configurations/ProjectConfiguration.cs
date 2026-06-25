using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain;

namespace TMS.Infrastruture
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.IsDeleted).HasDefaultValue(false);

            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
