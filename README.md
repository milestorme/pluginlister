# pluginlister
Displays installed plugins in-game and to discord

This plugin, PluginLister, is designed for use on Oxide-powered game servers (such as Rust, for example) to list all installed plugins and send this information to a specified Discord webhook.

Here's a detailed breakdown of the functionality:
1. Listing Installed Plugins
The plugin provides a command (/listplugins or /pluginlister.listplugins), which can be executed by an in-game player. When a player runs this command, the plugin collects and lists all the plugins currently installed on the server.
The list includes the names and versions of each plugin.
2. Sending Plugin Information to Discord
If enabled in the configuration, the plugin also sends the list of installed plugins to a Discord channel via a webhook.
The message sent to Discord includes the plugin name and version for each plugin, in a formatted message.
3. Configuration Options:
Enable Plugin: The plugin can be turned on or off. If it's disabled, the /listplugins command will inform the player that the plugin is not active.
Enable Discord Webhook: If this is set to true, the plugin will send the list of plugins to the Discord webhook. The webhook URL must be configured in the plugin’s settings.
Webhook URL: The Discord webhook URL where the plugin data will be sent.
4. Discord Message Formatting:
The message that is sent to Discord contains a list of installed plugins in a format like:
markdown

**Plugin Name** v1.0.0
**Another Plugin** v1.2.3


If there are too many plugins and the message exceeds Discord's 2000-character limit, the message will be split into multiple chunks and sent in several messages.
5. Error Handling:
The plugin includes basic error handling for the Discord webhook. If the webhook URL is not set or is incorrectly configured, it will print an error message in the server console.
If the Discord API responds with an error or the message cannot be sent, the plugin will log the failure in the console.
In summary:
The PluginLister plugin allows server admins or authorized players to list the installed plugins on an Oxide-powered server and have that list automatically posted to a Discord channel using a webhook. This helps admins keep track of the server’s plugins remotely through Discord, making server management easier.
