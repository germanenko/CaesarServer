using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_server_package;
using planner_server_package.Users;
using System.Collections.Generic;
using System.Text.Json;

namespace planner_chat_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NodeService> _logger;

        public NodeService(
            INodeRepository nodeRepository, IChatRepository chatRepository, ILogger<NodeService> logger, IUserService userService)
        {
            _nodeRepository = nodeRepository;
            _chatRepository = chatRepository;
            _logger = logger;
            _userService = userService;
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(Guid accountId, List<Guid> nodeIds)
        {
            var nodes = await _nodeRepository.GetNodesByIds(nodeIds);

            if (nodes.IsNullOrEmpty())
            {
                return new ServiceResponse<IEnumerable<NodeBody>>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }

            var chatBodies = new List<ChatBody>();
            var messageBodies = new List<MessageBody>();
            var others = new List<NodeBody>();

            foreach (var node in nodes)
            {
                if (node is Chat chat)
                {
                    var chatState = await _chatRepository.GetOrCreateChatState(chat.Id);
                    var chatUserState = await _chatRepository.GetOrCreateChatUserState(chat.Id, accountId);
                    var chatEdit = await _chatRepository.GetLastChatEdit(chat.Id);

                    var chatBody = chat.ToNodeBody();

                    chatBody.Name = chat.ChatSettings.FirstOrDefault(x => x.AccountId == accountId)?.ChatName ?? "Chat";
                    chatBody.State = chatState.ToBody();
                    chatBody.UserState = chatUserState.ToBody();
                    chatBody.ChatEdit = chatEdit?.ToBody();
                    chatBody.ChatSettings = chat.ChatSettings.FirstOrDefault(x => x.AccountId == accountId)?.ToBody();

                    var partnerId = chat.ChatSettings.FirstOrDefault(x => x.AccountId != accountId)?.AccountId;

                    if (partnerId.HasValue)
                        chatBody.Partner = await _userService.GetUserData(partnerId.Value);

                    chatBodies.Add(chatBody);
                }
                else if (node is ChatMessage chatMessage)
                {
                    if (!chatMessage.IsDeleted)
                        messageBodies.Add(chatMessage.ToNodeBody());
                }
                else
                {
                    others.Add(node.ToNodeBody());
                }
            }

            var result = others.Concat(chatBodies).Concat(messageBodies);

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId, List<Guid> rootIds)
        {
            var nodes = await _nodeRepository.GetNodes(accountId, rootIds);

            return new ServiceResponse<IEnumerable<Node>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes
            };
        }

        public async Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Guid accountId, Node node)
        {
            var newNode = await _nodeRepository.AddOrUpdateNode(accountId, node);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode.ToNodeBody()
            };
        }
    }
}
