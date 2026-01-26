using Microsoft.EntityFrameworkCore;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Infrastructure.Data;

namespace planner_content_service.Infrastructure.Repository
{
    public class PublicationStatusRepository : IPublicationStatusRepository
    {
        private readonly ContentDbContext _context;

        public PublicationStatusRepository(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<PublicationStatusModel?> ChangeStatus(Guid nodeId, PublicationStatus status)
        {
            var current = await _context.PublicationStatuses
                .Include(x => x.Node)
                .FirstOrDefaultAsync(x => x.NodeId == nodeId);

            if (current == null)
                return null;

            current.Status = status;
            await _context.SaveChangesAsync();

            return current;
        }

        public async Task<List<PublicationStatusModel>?> Get(PublicationStatus status)
        {
            var statuses = await _context.PublicationStatuses.Include(x => x.Node).Where(x => x.Status == status).ToListAsync();

            return statuses;
        }
    }
}