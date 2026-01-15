using Microsoft.EntityFrameworkCore;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Data
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

            modelBuilder.Entity<Node>().UseTptMappingStrategy();

            modelBuilder.Entity<Chat>().HasBaseType<Node>();
            modelBuilder.Entity<ChatMessage>().HasBaseType<Node>();

            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessages");
        }

        public DbSet<ChatSettings> ChatSettings { get; set; }
        public DbSet<AccountChatSession> AccountChatSessions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<AccessRight> AccessRights { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
    }
}