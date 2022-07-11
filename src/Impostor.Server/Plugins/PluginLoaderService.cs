using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Impostor.Server.Plugins
{
    public class PluginLoaderService : IHostedService
    {
        private readonly ILogger<PluginLoaderService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<PluginInformation> _plugins;

        public PluginLoaderService(ILogger<PluginLoaderService> logger, IServiceProvider serviceProvider, List<PluginInformation> plugins)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _plugins = plugins;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("加载插件喵！.");

            foreach (var plugin in _plugins)
            {
                _logger.LogInformation("启用了 {0}个插件.", plugin);

                // Create instance and inject services.
                plugin.Instance = (IPlugin)ActivatorUtilities.CreateInstance(_serviceProvider, plugin.PluginType);

                // Enable plugin.
                await plugin.Instance.EnableAsync();
            }

            _logger.LogInformation(
                _plugins.Count == 1
                    ? "加载了 {0} 插件."
                    : "加载了 {0} 插件.",
                _plugins.Count
            );
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Disable all plugins with a valid instance set.
            // In the case of a failed startup, some can be null.
            foreach (var plugin in _plugins)
            {
                if (plugin.Instance != null)
                {
                    _logger.LogInformation("禁用插件 {0}个.", plugin);

                    // Disable plugin.
                    await plugin.Instance.DisableAsync();
                }
            }
        }
    }
}
