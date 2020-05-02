using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace vBot.modules.games.connect4
{
    public class Connect4Commands : ModuleBase<SocketCommandContext>
    {
        [Command("c4")]
        public async Task C4Start(SocketGuildUser user)
        {
            if (user.Id == Context.User.Id || user.IsBot)
            {
                await ReplyAsync("Invalid user");
                return;
            }

            Connect4 game = new Connect4(Context.User as SocketGuildUser, user, Context.Channel);
            await Task.CompletedTask;
        }
    }
}
