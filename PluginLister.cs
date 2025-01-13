using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("PluginLister", "Milestorme", "1.3.5")]
    [Description("Lists installed plugins and sends the data to a Discord webhook.")]
    public class PluginLister : CovalencePlugin
    {
        private PluginConfig config;

        private class PluginConfig
        {
            public bool EnablePlugin { get; set; } = true; // Enable or disable the plugin entirely
            public bool EnableDiscordWebhook { get; set; } = true; // Enable or disable sending to Discord
            public string WebhookUrl { get; set; } = "YOUR_DISCORD_WEBHOOK_URL_HERE"; // Discord webhook URL
        }

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
            // Create a custom permission group for PluginLister admins
            if (!permission.GroupExists("pluginlister.admin"))
            {
                // The third parameter `0` is the rank (you can change it if needed)
                permission.CreateGroup("pluginlister.admin", "admin", 0); 
                Puts("Created 'pluginlister.admin' permission group.");
            }

            // Grant the group the necessary permission
            if (!permission.PermissionExists("pluginlister.listplugins"))
            {
                permission.RegisterPermission("pluginlister.listplugins", this);
                Puts("Registered 'pluginlister.listplugins' permission.");
            }
        }

        [Command("listplugins", "pluginlister.listplugins")]
        private void ListPluginsCommand(IPlayer player, string command, string[] args)
        {
            // Check if the player has admin rights
            if (!player.HasPermission("pluginlister.admin") && !player.IsAdmin)
            {
                player.Reply("You do not have permission to use this command.");
                return;
            }

            if (!config.EnablePlugin)
            {
                player.Reply("This plugin is currently disabled.");
                return;
            }

            // Get installed plugins
            var plugins = Interface.Oxide.RootPluginManager.GetPlugins().ToList(); // Convert IEnumerable to List<Plugin>
            var pluginNames = new List<string>();
            foreach (var plugin in plugins)
            {
                pluginNames.Add(plugin.Title); // Only add plugin name for in-game output
            }

            string pluginList = string.Join("\n", pluginNames); // Join plugin names with new lines

            // Send response to player (only plugin names in-game)
            player.Reply($"Installed Plugins:\n{pluginList}");

            // Send plugin list to Discord if enabled
            if (config.EnableDiscordWebhook)
            {
                SendToDiscord(plugins); // Send full plugin info (name + version) to Discord
            }
        }

        private void SendToDiscord(List<Plugin> plugins)
        {
            if (string.IsNullOrEmpty(config.WebhookUrl) || config.WebhookUrl == "YOUR_DISCORD_WEBHOOK_URL_HERE")
            {
                Puts("Webhook URL is not configured or is using the default placeholder.");
                return;
            }

            // Build the message content for Discord (with plugin names and version numbers)
            var pluginNamesWithVersions = new List<string>();
            foreach (var plugin in plugins)
            {
                // Make plugin name bold and version normal
                pluginNamesWithVersions.Add($"**{plugin.Title}** v{plugin.Version}");
            }

            string messageContent = $"Installed Plugins:\n{string.Join("\n", pluginNamesWithVersions)}";

            // Split the message into chunks if it exceeds 2000 characters
            var messages = SplitMessage(messageContent);

            // Send each chunk to Discord
            foreach (var message in messages)
            {
                SendMessageToDiscord(message);
            }
        }

        private List<string> SplitMessage(string message)
        {
            const int maxLength = 2000;
            var messages = new List<string>();

            while (message.Length > maxLength)
            {
                // Find the last full line within the max length
                int splitIndex = message.LastIndexOf("\n", maxLength, StringComparison.OrdinalIgnoreCase);
                if (splitIndex == -1) splitIndex = maxLength;

                // Split the message into chunks and add to the list
                messages.Add(message.Substring(0, splitIndex).Trim());
                message = message.Substring(splitIndex).Trim();
            }

            // Add the remaining part of the message
            if (message.Length > 0)
                messages.Add(message);

            return messages;
        }

        private void SendMessageToDiscord(string message)
        {
            var payload = new
            {
                content = message
            };

            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            // Send request to Discord
            webrequest.Enqueue(
                config.WebhookUrl,
                JsonConvert.SerializeObject(payload),
                (code, response) =>
                {
                    if (code != 200 || response == null)
                    {
                        Puts($"Failed to send data to Discord. Response code: {code}. Response: {response}");
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

        // This method will be called when the plugin is loaded
        private void OnServerInitialized()
        {
            CreatePermissionGroup(); // Create the permission group when the plugin is loaded
        }
    }
}
