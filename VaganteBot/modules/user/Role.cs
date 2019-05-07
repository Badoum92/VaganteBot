using System;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;

namespace VaganteBot.modules.user
{
    public class Role :  ModuleBase<SocketCommandContext>
    {
        public static SocketRole GetHighestRole(SocketGuildUser user)
        {
            return user.Roles.OrderByDescending(r => r.Position).First();
        }
    }
}