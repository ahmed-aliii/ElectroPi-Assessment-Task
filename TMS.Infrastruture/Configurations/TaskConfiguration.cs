using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainTask = TMS.Domain.Task;

namespace TMS.Infrastruture
{
    public class TaskConfiguration : IEntityTypeConfiguration<DomainTask>
    {
        public void Configure(EntityTypeBuilder<DomainTask> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(2000);

            builder.Property(t => t.Status)
                .IsRequired();

            builder.Property(t => t.Priority)
                .IsRequired();

            builder.Property(t => t.ProjectId)
                .IsRequired();

            builder.HasIndex(t => t.ProjectId);
        }
    }
}
