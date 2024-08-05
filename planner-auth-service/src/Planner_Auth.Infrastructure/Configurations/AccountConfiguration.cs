using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_Auth.Core.Entities.Models;

namespace Planner_Auth.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Identifier)
            .IsRequired()
            .HasMaxLength(256);

            builder.Property(a => a.Nickname)
                .IsRequired();

            builder.Property(a => a.RoleName)
                .IsRequired();

            builder.Property(a => a.PasswordHash)
                .IsRequired();

            builder.Property(a => a.AuthenticationMethod)
                .IsRequired();

            builder.Property(a => a.AuthorizationProvider)
                .IsRequired();

            builder.HasMany(a => a.Sessions)
                .WithOne(s => s.Account)
                .HasForeignKey(s => s.AccountId);
        }
    }
}