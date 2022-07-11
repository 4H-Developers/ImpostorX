using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Server.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Impostor.Server.Net
{
    internal class MatchmakerService : IHostedService
    {
        private readonly ILogger<MatchmakerService> _logger;
        private readonly ServerConfig _serverConfig;
        private readonly ServerRedirectorConfig _redirectorConfig;
        private readonly Matchmaker _matchmaker;

        public MatchmakerService(
            ILogger<MatchmakerService> logger,
            IOptions<ServerConfig> serverConfig,
            IOptions<ServerRedirectorConfig> redirectorConfig,
            Matchmaker matchmaker)
        {
            _logger = logger;
            _serverConfig = serverConfig.Value;
            _redirectorConfig = redirectorConfig.Value;
            _matchmaker = matchmaker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_serverConfig.ResolveListenIp()), _serverConfig.ListenPort);

            await _matchmaker.StartAsync(endpoint);

            _logger.LogInformation(
                "Matchmaker已开启监听 {0}:{1}, 公共服务器ip为 {2}:{3}.老李提醒您关防火墙不规范 内鬼两行泪",
                endpoint.Address,
                endpoint.Port,
                _serverConfig.ResolvePublicIp(),
                _serverConfig.PublicPort);

            if (_redirectorConfig.Enabled)
            {
                _logger.LogWarning(_redirectorConfig.Master
                    ? "Server redirection is enabled as master, this instance will redirect clients to other nodes."
                    : "Server redirection is enabled as node, this instance will accept clients.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Matchmaker is shutting down!");
            await _matchmaker.StopAsync();
        }
    }
}
