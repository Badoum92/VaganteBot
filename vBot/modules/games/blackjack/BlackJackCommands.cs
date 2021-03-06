using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using vBot.modules.user;

namespace vBot.modules.games.blackjack
{
    public class BlackJackCommands : ModuleBase<SocketCommandContext>
    {
        [Command("blackjack")]
        public async Task BlackJackStart(int bet = 0)
        {
            if (bet < 0 || !Gold.HasEnoughGold(Context.User.Id, bet))
            {
                await ReplyAsync("Invalid bet");
                return;
            }

            BlackJack game = new BlackJack(Context.User as SocketGuildUser, bet, Context.Channel);
            await Task.CompletedTask;
        }

        [Command("bj")]
        public async Task BJStart(int bet = 0)
        {
            await BlackJackStart(bet);
        }
    }
}