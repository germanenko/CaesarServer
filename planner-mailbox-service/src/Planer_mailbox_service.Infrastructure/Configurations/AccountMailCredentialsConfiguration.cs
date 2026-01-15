using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_mailbox_service.Core.Entities.Models;

namespace Planer_mailbox_service.Infrastructure.Configurations
{
    public class AccountMailCredentialsConfiguration : IEntityTypeConfiguration<AccountMailCredentials>
    {
        public void Configure(EntityTypeBuilder<AccountMailCredentials> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Email);
            builder.Property(e => e.AccessToken)
                .IsRequired();

            builder.Property(e => e.EmailProvider)
                .IsRequired();

        }
    }
}