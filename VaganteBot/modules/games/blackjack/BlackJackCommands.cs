using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using VaganteBot.modules.user;

namespace VaganteBot.modules.games.blackjack
{
    public class BlackJackCommands : ModuleBase<SocketCommandContext>
    {
        [Command("blackjack")]
        public async Task BlackJackStart(int bet)
        {
            if (bet < 0 || !Gold.HasEnoughGold(Context.User.Id, bet))
            {
                await ReplyAsync("Invalid bet");
                return;
            }

            BlackJack game = new BlackJack(Context.User as SocketGuildUser, bet, Context.Channel);
            await Task.CompletedTask;
        }
    }
}