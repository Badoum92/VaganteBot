using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;

namespace VaganteBot
{
    public class RandomCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task ping()
        {
            await ReplyAsync("pong");
        }
    }
}
