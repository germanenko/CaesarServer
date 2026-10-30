using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Api.CustomAttributes;
using planner_node_service.Api.Stubs;
using planner_node_service.Core.IService;
using planner_server_package;
using planner_server_package.Idempotency.Interface;
using Sprache;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace planner_node_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class NodeController : ControllerBase
    {
        private readonly INodeService _nodeService;
        private readonly IJwtService _jwtService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly TestStubs _stubs;


        public NodeController(
            INodeService nodeService,
            IJwtService jwtService,
            IIdempotencyService idempotencyService,
            TestStubs stubs)
        {
            _nodeService = nodeService;
            _jwtService = jwtService;
            _idempotencyService = idempotencyService;
            _stubs = stubs;
        }

        [LocalOnly]
        [HttpPost("createOrUpdateNode")]
        [SwaggerOperation("Создать/обновить ноду")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateOrUpdateNode(
            [FromQuery] Guid accountId,
            [FromQuery] OperationName operationName,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromBody] NodeBody nodeBody,
            CancellationToken cancellationToken
        )
        {
            //var result = await _nodeService.AddOrUpdateNode(accountId, nodeBody);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                accountId,
                operationName,
                nodeBody,
                async () => await _nodeService.AddOrUpdateNode(accountId, nodeBody),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<ServiceResponse<NodeBody>>() { Body = result });

            return StatusCode((int)result.StatusCode, new Response<ServiceResponse<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getNodeBranchesByRootIds"), Authorize]
        [SwaggerOperation("Получить ветки нод по Id корней")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetNodeBranchesByRootIds(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> rootIds,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodes(tokenPayload.AccountId, rootIds);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getNodesByIds"), Authorize]
        [SwaggerOperation("Получить ноды по Id")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetNodesByIds(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> nodeIds,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodesByIds(tokenPayload.AccountId, nodeIds);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getManifest"), Authorize]
        [SwaggerOperation("Получить манифест")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetManifest(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> scopeIds,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetManifests(tokenPayload.AccountId, scopeIds);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getScopesManifest"), Authorize]
        [SwaggerOperation("Получить Scopes манифест")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetScopesManifest(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetScopesManifest(tokenPayload.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getScopeManifest"), Authorize]
        [SwaggerOperation("Получить Scope манифест")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetScopeManifest(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid scopeId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetScopeManifest(tokenPayload.AccountId, scopeId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<EntityVersionBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("changeNodeParent"), Authorize]
        [SwaggerOperation("Изменить родителя ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> ChangeNodeParent(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid nodeId,
            [FromQuery] Guid newParentId,
            [FromQuery] int position,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _nodeService.ChangeNodeParent(tokenPayload.AccountId, nodeId, newParentId, position);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.ChangeNodeParent,
                (nodeId, newParentId, position),
                async () => await _nodeService.ChangeNodeParent(tokenPayload.AccountId, nodeId, newParentId, position),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<NodeLinkBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<NodeLinkBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }


        [HttpPost("changeNodePosition"), Authorize]
        [SwaggerOperation("Изменить позицию ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> ChangeNodePosition(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid nodeId,
            [FromQuery] int newPosition,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _nodeService.ChangeNodePosition(tokenPayload.AccountId, nodeId, newPosition);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.ChangeNodePosition,
                (nodeId, newPosition),
                async () => await _nodeService.ChangeNodePosition(tokenPayload.AccountId, nodeId, newPosition),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<NodeLinkBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<NodeLinkBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }


        [HttpPost("sendLocalNodes"), Authorize]
        [SwaggerOperation("Отправить локальные ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> SendLocalNodes(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromBody] List<NodeBody> nodes,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _nodeService.LoadNodes(nodes, tokenPayload);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.SendLocalUpdates,
                nodes,
                async () => await _nodeService.LoadNodes(nodes, tokenPayload),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getNodesByTypes"), Authorize]
        [SwaggerOperation("Получить ноды по типу")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodesByTypes(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<NodeType> nodeTypes,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodesByTypes(tokenPayload.AccountId, nodeTypes);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}