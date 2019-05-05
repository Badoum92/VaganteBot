using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace VaganteBot
{
    public class TicTacToeCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ttt")]
        public async Task TTTStart(SocketGuildUser user)
        {
            if (user.Id == Context.User.Id || user.IsBot)
            {
                await ReplyAsync("Ivalid user");
                return;
            }

            TicTacToe game = new TicTacToe(Context.User as SocketGuildUser, user, Context.Channel);
            await Task.CompletedTask;
        }
    }
}
