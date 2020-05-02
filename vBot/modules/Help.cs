using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace vBot.modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpCmd()
        {
            string help = @"◽ **Help** ◽

Help page about this bot's features.  

▫️ |  `v!help general` \➖ General purpose commands
▫️ |  `v!help gold` \➖ vBot's gold system
▫️ |  `v!help games` \➖ Games you can play with this bot
▫️ |  `v!help roles` \➖ Infos about users and their roles
▫️ |  `v!help challenges` \➖ Infos about challenges
▫️ |  `v!help music` \➖ Listen to music in voice chat";

            await ReplyAsync(help);
        }

        [Command("help general")]
        public async Task HelpGeneral()
        {
            string help = @"◽ **Help General** ◽

General purpose commands.  

▫️ |  `v!ping` \➖ Checks if the bot is alive
▫️ |  `v!profile [user]` \➖ Shows `user`'s profile (default=self)
▫️ |  `v!avatar [user]` \➖ Shows `user`'s avatar (default=self)";

            await ReplyAsync(help);
        }

        [Command("help gold")]
        public async Task HelpGold()
        {
            string help = @"◽ **Help Gold** ◽

vBot's gold system.

▫️ |  `v!gold [user]` \➖ Shows the amount of gold `user` has (default=self)
▫️ |  `v!chest` \➖ Opens your daily chest and gives you some gold
▫️ |  `v!give user amount` \➖ Gives `amount` of your gold to `user`";

            await ReplyAsync(help);
        }

        [Command("help games")]
        public async Task HelpGames()
        {
            string help = @"◽ **Help Games** ◽

Games you can play with this bot.

▫️ |  `v!ttt user` \➖ Starts a game of tic-tac-toe with `user`
▫️ |  `v!c4 user` \➖ Starts a game of connect4 with `user`
▫️ |  `v!blackjack [bet]` \➖ Starts a game of blackjack with `bet` gold (default=0)
▫️ |  `v!bj [bet]` \➖ Starts a game of blackjack with `bet` gold (default=0)";

            await ReplyAsync(help);
        }

        [Command("help roles")]
        public async Task HelpRoles()
        {
            string help = @"◽ **Help Roles** ◽

Infos about users and their roles.

▫️ |  `v!userroles [user]` \➖ Shows every roles of `user` (default=self)
▫️ |  `v!roleusers rolename` \➖ Shows every users with role `rolename`";

            await ReplyAsync(help);
        }

        [Command("help challenges")]
        public async Task HelpChallenges()
        {
            string help = @"◽ **Help Roles** ◽

Infos about challenges.
Spreadsheet: <http://goo.gl/cu2g11>

▫️ |  `v!challenge name` \➖ Shows info about challenge `name`
▫️ |  `v!challengerole rolename` \➖ Shows infos about the challenge corresponding to the role `rolename`";

            await ReplyAsync(help);
        }

        [Command("help music")]
        public async Task HelpMusic()
        {
            string help = @"◽ **Help Music** ◽

Listen to music in voice chat.

▫️ |  `v!addsong url` \➖ Queues the song corresponding to the provided **youtube** `url`
▫️ |  `v!current` \➖ Shows infos about the current song
▫️ |  `v!remaining` \➖ Shows the remaining time of the current song
▫️ |  `v!next` \➖ Shows the 10 following songs
▫️ |  `v!skip` \➖ Vote to skip the current song";

            await ReplyAsync(help);
        }
    }
}
