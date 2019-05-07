using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace VaganteBot.modules.games.tictactoe
{
    public class TicTacToeCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ttt")]
        public async Task TttStart(SocketGuildUser user)
        {
            if (user.Id == Context.User.Id || user.IsBot)
            {
                await ReplyAsync("Invalid user");
                return;
            }

            TicTacToe game = new TicTacToe(Context.User as SocketGuildUser, user, Context.Channel);
            await Task.CompletedTask;
        }
    }
}
