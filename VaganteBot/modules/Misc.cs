using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
namespace VaganteBot.modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public static void DownloadImage(string url, string path)
        {
            using (WebClient client = new WebClient()) 
            {
                client.DownloadFileAsync(new Uri(url), path);
            }
        }

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("pong");
        }

        [Command("delete")]
        public async Task DeteleMessages(int n)
        {
            if (Context.User.Id != Program.owner)
                return;

            var messages = await Context.Channel.GetMessagesAsync(n + 1).FlattenAsync();
            foreach (var m in messages)
            {
                await m.DeleteAsync();
            }
        }
    }
}
