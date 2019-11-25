using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace vBot.modules
{
    public class Role :  ModuleBase<SocketCommandContext>
    {
        public static SocketRole GetHighestRole(SocketGuildUser user)
        {
            return user.Roles.OrderByDescending(r => r.Position).First();
        }
        
        [Command("userroles")]
        public async Task UserRoles(SocketGuildUser user = null)
        {
            user = user ?? Context.User as SocketGuildUser;

            string text = "Roles of user " + Util.FormatText(user.Username, "**") + ":\n";
            var roles = user.Roles.ToList().OrderByDescending(r => r.Position);
            foreach (var role in roles)
            {
                if (role.Name == "@everyone")
                    continue;
                text += ":white_small_square: " + role.Name + "\n";
                if (text.Length < 1900)
                    continue;
                await ReplyAsync(text);
            }
            if (text != "")
                await ReplyAsync(text);
        }
        
        [Command("roleusers")]
        public async Task RoleUsers([Remainder] string roleName)
        {
            var role = Util.GetClosestElement(roleName, Context.Guild.Roles, r => r.Name);
            string text = "Users with role " + Util.FormatText(role.Name, "**") + ":\n";
            foreach (var user in role.Members)
            {
                text += ":white_small_square: " + user.Username + "\n";
                if (text.Length < 1900)
                    continue;
                await ReplyAsync(text);
                text = "";
            }

            if (text != "")
                await ReplyAsync(text);
        }
    }
}