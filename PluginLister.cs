// Copyright (c) 2025 Milestorme
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is provided
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("PluginLister", "Milestorme", "1.3.3")]
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

        [Command("listplugins", "pluginlister.listplugins")]
        private void ListPluginsCommand(IPlayer player, string command, string[] args)
        {
            if (!config.EnablePlugin)
            {
                player.Reply("This plugin is currently disabled.");
                return;
            }

            // Get installed plugins
            var plugins = 
