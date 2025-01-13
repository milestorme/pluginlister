using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("PluginLister", "Milestorme", "1.3.7")]
    [Description("Lists installed plugins and sends the data to a Discord webhook.")]
    public class PluginLister : CovalencePlugin
    {
        private PluginConfig config;
        private Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        private class PluginConfig
        {
            public bool EnablePlugin { get; set; } = true;
            public bool EnableDiscordWebhook { get; set; } = true;
            public string WebhookUrl { get; set; } = "YOUR_DISCORD_WEBHOOK_URL_HERE";
            public int CommandCooldownSeconds { get; set; } = 30; // Cooldown in seconds
        }

        private static readonly Dictionary<string, string> Localization = new Dictionary<string, string>
        {
            { "NoPermission", "You do not have permission to use this command." },
            { "PluginDisabled", "This plugin is currently disabled." },
            { "NoPlugins", "No plugins are currently installed." },
            { "CooldownMessage", "You must wait {0} seconds before using this command again." },
            { "PluginsListed", "Installed Plugins ({0}):\n{1}" }
        };

        protected override void LoadDefaultConfig()
        {
            config = new PluginConfig();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<PluginConfig>();
                if (config == null) throw new Exception("Configuration file is null");
            }
            catch
            {
                PrintError("Configuration file is corrupted or missing, creating a new one.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(config, true);

        private void CreatePermissionGroup()
        {
            if (!permission.GroupExists("pluginlister.admin"))
            {
                permission.CreateGroup("pluginlister.admin", "admin", 0);
                Puts("Created 'pluginlister.admin' permission group.");
            }

            if (!permission.PermissionExists("pluginlister.listplugins"))
            {
                permission.RegisterPermission("pluginlister.listplugins", this);
                Puts("Registered 'pluginlister.listplugins' permission.");
            }
        }

        [Command("listplugins", "pluginlister.listplugins")]
        private void ListPluginsCommand(IPlayer player, string command, string[] args)
        {
            if (!player.HasPermission("pluginlister.admin") && !player.IsAdmin)
            {
                player.Reply(Localization["NoPermission"]);
                return;
            }

            if (!config.EnablePlugin)
            {
                player.Reply(Localization["PluginDisabled"]);
                return;
            }

            if (cooldowns.ContainsKey(player.Id) && cooldowns[player.Id] > DateTime.Now)
            {
                var remaining = (cooldowns[player.Id] - DateTime.Now).TotalSeconds;
                player.Reply(string.Format(Localization["CooldownMessage"], Math.Ceiling(remaining)));
                return;
            }

            cooldowns[player.Id] = DateTime.Now.AddSeconds(config.CommandCooldownSeconds);

            var plugins = Interface.Oxide.RootPluginManager.GetPlugins().ToList();
            if (!plugins.Any())
            {
                player.Reply(Localization["NoPlugins"]);
                return;
            }

            var pluginNames = plugins.Select(p => p.Title).ToList();
            var pluginList = string.Join("\n", pluginNames);
            player.Reply(string.Format(Localization["PluginsListed"], pluginNames.Count, pluginList));

            if (config.EnableDiscordWebhook)
            {
                SendToDiscord(plugins);
            }
        }

        private void SendToDiscord(List<Plugin> plugins)
        {
            if (string.IsNullOrEmpty(config.WebhookUrl) || config.WebhookUrl == "YOUR_DISCORD_WEBHOOK_URL_HERE")
            {
                Puts("Webhook URL is not configured or is using the default placeholder.");
                return;
            }

            var pluginDetails = plugins.Select(p => new { Name = p.Title, Version = p.Version }).ToList();
            var embed = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = "Installed Plugins",
                        description = string.Join("\n", pluginDetails.Select(p => $"**{p.Name}** v{p.Version}")),
                        color = 3447003
                    }
                }
            };

            SendDiscordPayload(JsonConvert.SerializeObject(embed));
        }

        private void SendDiscordPayload(string payload, int retryCount = 0)
        {
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            webrequest.Enqueue(
                config.WebhookUrl,
                payload,
                (code, response) =>
                {
                    if (code != 200 && code != 204)
                    {
                        Puts($"Failed to send data to Discord. Response code: {code}. Response: {response}");

                        if (retryCount < 3) // Retry up to 3 times
                        {
                            timer.Once(5f, () => SendDiscordPayload(payload, retryCount + 1));
                        }
                    }
                    else
                    {
                        Puts("Successfully sent data to Discord!");
                    }
                },
                this,
                Oxide.Core.Libraries.RequestMethod.POST,
                headers
            );
        }

        private void OnServerInitialized()
        {
            CreatePermissionGroup();
        }
    }
}
