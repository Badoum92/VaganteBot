using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace VaganteBot.modules.challenge
{
    public class Challenge : ModuleBase<SocketCommandContext>
    {
        public class ChallengeInstance
        {
            public string title;
            public string role;
            public string description;
            public string currentVersion;
            public string character;
            public string singlemulti;
            public string background;
            public string areaGoal;
            public string poster;
            public string hex;

            public Discord.Color GetColor()
            {
                string rStr = hex[1].ToString() + hex[2].ToString();
                string gStr = hex[3].ToString() + hex[4].ToString();
                string bStr = hex[5].ToString() + hex[6].ToString();

                int r = Int32.Parse(rStr, System.Globalization.NumberStyles.HexNumber);
                int g = Int32.Parse(gStr, System.Globalization.NumberStyles.HexNumber);
                int b = Int32.Parse(bStr, System.Globalization.NumberStyles.HexNumber);
                
                return new Color(r, g, b);
            }
        }

        private static List<ChallengeInstance> challenges;

        public static void Init()
        {
            string json = File.ReadAllText("data/challenges.json");
            challenges = JsonConvert.DeserializeObject<List<ChallengeInstance>>(json);
        }

        public static ChallengeInstance GetChallengeByTitle(string title)
        {
            int dist = Int32.MaxValue;
            ChallengeInstance challenge = null;
            title = title.ToLower();
            foreach (var c in challenges)
            {
                int tmp = Util.Levenshtein(title, c.title.ToLower());
                if (tmp == 0)
                    return c;
                if (tmp >= dist)
                    continue;
                dist = tmp;
                challenge = c;
            }
            return challenge;
        }
        
        public static ChallengeInstance GetChallengeByRole(string role)
        {
            int dist = Int32.MaxValue;
            ChallengeInstance challenge = null;
            role = role.ToLower();
            foreach (var c in challenges)
            {
                int tmp = Util.Levenshtein(role, c.role.ToLower());
                if (tmp == 0)
                    return c;
                if (tmp >= dist)
                    continue;
                dist = tmp;
                challenge = c;
            }
            return challenge;
        }

        public static Embed MakeEmbed(ChallengeInstance challenge)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(challenge.title);
            embed.WithColor(challenge.GetColor());
            embed.AddField("Role", challenge.role);
            embed.AddField("Description", Util.GetNFirstChars(challenge.description, 1500));
            embed.AddField("Class", challenge.character);
            embed.AddField("Single/Multi", challenge.singlemulti);
            embed.AddField("Background", challenge.background);
            embed.AddField("Area Goal", challenge.areaGoal);
            embed.AddField("Poster", challenge.poster);
            return embed.Build();
        }
        
        [Command("challenge")]
        public async Task Challenge_([Remainder] string challengeName)
        {
            var challenge = Util.GetClosestElement(challengeName, challenges, c => c.title);
            await Context.Channel.SendMessageAsync(null, false, MakeEmbed(challenge));
        }
        
        [Command("challengerole")]
        public async Task ChallengeRole([Remainder] string roleName)
        {
            var challenge = Util.GetClosestElement(roleName, challenges, c => c.role);
            await Context.Channel.SendMessageAsync(null, false, MakeEmbed(challenge));
        }

        [Command("monthlychallenge")]
        public async Task MonthlyChallenge()
        {
            var user = Context.User as SocketGuildUser;
            if (!User.HasRole(user, 223128952426856451) && !User.HasRole(user, 482825446753566721))
                return;

            Random rand = new Random();
            int index = rand.Next(0, challenges.Count);
            ChallengeInstance challenge = challenges[index];
            await Context.Channel.SendMessageAsync(null, false, MakeEmbed(challenge));
        }
    }
}