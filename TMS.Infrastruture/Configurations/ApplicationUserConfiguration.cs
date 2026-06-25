using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Infrastruture
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Simple string attributes
            builder.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        }
    }
}
