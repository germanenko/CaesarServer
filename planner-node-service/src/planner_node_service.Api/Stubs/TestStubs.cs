using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_server_package.Stubs;

namespace planner_node_service.Api.Stubs
{
    public class TestStubKeys
    {
        public const string GetNodesByIdsInternalError = "node.getNodesByIds.internalError";
    }


    public sealed class TestStubs
    {
        private readonly Dictionary<string, TestStub> _active = new();

        // объявление всех заглушек - единственное место в коде
        private static readonly TestStub[] _known =
        {
            new(TestStubKeys.GetNodesByIdsInternalError,  ErrorCode.Infrastructure,     "даниил, тест отката удаления",  new(2026, 11, 1)),
        };

        private readonly ILogger<TestStubs> _logger;

        public TestStubs(IConfiguration config, IHostEnvironment env, ILogger<TestStubs> logger)
        {
            _logger = logger;

            foreach (var stub in _known)
            {
                if (!config.GetValue<bool>($"TestStubs:{stub.Key}")) continue;

                // вне dev не стартует
                if (!env.IsDevelopment())
                    throw new InvalidOperationException($"тестовая заглушка {stub.Key} включена в {env.EnvironmentName}");

                // просроченная работает, но орёт в лог при каждом старте; снимает человек
                if (DateOnly.FromDateTime(DateTime.UtcNow) > stub.RemoveOn)
                {
                    _logger.LogError("заглушка {Key} просрочена с {Date}: {Reason}", stub.Key, stub.RemoveOn, stub.Reason);
                }

                _active[stub.Key] = stub;
            }
        }

        public bool TryGet(string key, out TestStub stub) => _active.TryGetValue(key, out stub);
    }
}
