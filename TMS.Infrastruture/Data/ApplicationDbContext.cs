using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Domain;

namespace TMS.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        #region CTORs

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        #endregion CTORs

        #region DbSets

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TMS.Domain.Task> Tasks => Set<TMS.Domain.Task>();

        #endregion DbSets

        #region Fluent API

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        #endregion Fluent API
    }
}
