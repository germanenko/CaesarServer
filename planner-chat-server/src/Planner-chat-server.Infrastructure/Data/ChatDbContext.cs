using Microsoft.EntityFrameworkCore;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
        }

        public DbSet<ChatMembership> ChatMemberships { get; set; }
        public DbSet<AccountChatSession> AccountChatSessions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}