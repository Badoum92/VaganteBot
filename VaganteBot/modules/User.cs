using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace VaganteBot.modules
{
    public class User : ModuleBase<SocketCommandContext>
    {
        public static Discord.Color GetUserColor(SocketGuildUser user)
        {
            return Role.GetHighestRole(user).Color;
        }
        
        [Command("avatar")]
        public async Task Avatar(SocketGuildUser user = null)
        {
            user = user ?? Context.User as SocketGuildUser;

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(Util.FormatText(user.Username, "**"));
            builder.WithImageUrl(user.GetAvatarUrl());
            builder.WithColor(GetUserColor(user));

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("profile")]
        public async Task Profile(SocketGuildUser user = null)
        {
            user = user ?? Context.User as SocketGuildUser;

            var highestRole = Role.GetHighestRole(user);
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(Util.FormatText(user.Username, "**"));
            builder.WithThumbnailUrl(user.GetAvatarUrl());
            builder.WithColor(highestRole.Color);
            builder.AddField("Account created on", user.JoinedAt);
            builder.AddField("Join date", user.CreatedAt);
            builder.AddField("Number of roles", user.Roles.Count - 1);
            builder.AddField("Highest role", highestRole.Name);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}