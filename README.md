This plugin, named PluginLister, is designed for Oxide-based servers (like Rust) and performs the following tasks:

**Key Functions:**
List Installed Plugins:

The plugin allows users to view a list of all installed plugins on the server. This can be done by using the command listplugins in the game.
Admin Permission Required:
Only players with specific permissions (`pluginlister.admin`) can use the command. This ensures that only authorized users (like server admins) can access the plugin list.

**Discord Integration:**

The plugin can send a list of installed plugins (including their names and versions) to a Discord channel via a webhook. This is useful for admins who want to keep track of the server's plugins externally.
The webhook URL is configurable, allowing you to send the data to any Discord channel of your choice.
Custom Permission Group:

The plugin automatically creates a custom permission group named pluginlister.admin. Players assigned to this group will be able to execute the listplugins command.
This permission group is created upon server initialization, and the necessary permissions are granted to the group.

**How It Works:**

Command Execution:

When a player types the listplugins command, the plugin checks if the player has the necessary permission (pluginlister.admin). If the player is authorized, it lists all installed plugins in the game.
Sending Data to Discord:

If the webhook URL is set and active, the plugin sends a message to a specified Discord channel. The message includes the list of installed plugins, along with their version numbers.
The message is split into chunks if it exceeds Discord's message size limit (2000 characters).

**Permission Management:**

The plugin automatically creates the permission group pluginlister.admin if it doesn’t exist. It also ensures that the necessary permissions (pluginlister.listplugins) are available for players in this group.
Server admins can assign this permission group to players using commands like oxide.grant.

**Configuration:**

Plugin Enable/Disable:
You can enable or disable the plugin entirely using a configuration setting (EnablePlugin).
Discord Webhook:
You need to provide a valid Discord webhook URL in the config file (WebhookUrl) to enable sending plugin data to Discord.

**Benefits:**

Ease of Use: Admins can easily see the plugins running on their server via the in-game command or on Discord.
Automation: The plugin automatically handles permissions and sends data to Discord without requiring additional manual steps after setup.
Security: Only players with the correct permission can execute the command, making it secure for server administration.

**Example Use:**

Assign Permissions: You need to grant the pluginlister.admin permission to players who should be able to view the plugin list.


`oxide.grant user <player_name> pluginlister.admin`
Use Command: Players with the pluginlister.admin permission can run:

`/listplugins`
This will show them a list of installed plugins in the game.

View on Discord: If the Discord webhook is configured, the plugin will also send the list of installed plugins to the specified Discord channel.

**Summary:**

The PluginLister plugin is a useful tool for server administrators, allowing them to easily view and share a list of installed plugins on their server both in-game and on Discord. It requires minimal setup and ensures that only authorized users can access sensitive plugin information.