using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Planer_mailbox_service.Core.Entities.Models;

namespace Planer_mailbox_service.Infrastructure.Data
{
    public class AccountCredentialsDbContext : DbContext
    {
        public AccountCredentialsDbContext(DbContextOptions<AccountCredentialsDbContext> options) : base(options)
        {
        }

        public DbSet<AccountMailCredentials> MailCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}