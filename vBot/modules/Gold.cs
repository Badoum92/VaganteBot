using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace vBot.modules.user
{
    public class Gold : ModuleBase<SocketCommandContext>
    {
        public class GoldUser
        {
            public int gold;
            public DateTime lastChest;
            public ulong id;

            public GoldUser(int gold_, DateTime lastChest_, ulong id_)
            {
                gold = gold_;
                lastChest = lastChest_;
                id = id_;
            }

            public void AddGold(int amount)
            {
                gold = Math.Max(gold + amount, 0);
                Update();
            }

            public void Update()
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(GetPath(id), json);
            }
        }

        private static string GetPath(ulong id)
        {
            return "data/gold/" + id + ".json";
        }

        public static GoldUser GetUser(ulong id)
        {
            string path = GetPath(id);
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                GoldUser newUser = new GoldUser(1000, DateTime.Today.AddDays(-1), id);
                newUser.Update();
                return newUser;
            }

            string json = File.ReadAllText(path);
            GoldUser user = JsonConvert.DeserializeObject<GoldUser>(json);
            return user;
        }

        public static void AddGold(ulong id, int amount)
        {
            GetUser(id).AddGold(amount);
        }

        public static bool HasEnoughGold(ulong id, int amount)
        {
            var goldUser = GetUser(id);
            return goldUser.gold >= amount;
        }
        
        private static int Chest(GoldUser user)
        {
            if (user.lastChest == DateTime.Today)
                return -1;
            Random rand = new Random();
            int amount = rand.Next(200, 501);
            user.lastChest = DateTime.Today;
            user.AddGold(amount);
            return amount;
        }

        [Command("gold")]
        public async Task ShowGold(SocketGuildUser user = null)
        {
            user = user ?? Context.User as SocketGuildUser;

            if (user.IsBot)
            {
                await ReplyAsync("Bots don't have gold.");
                return;
            }

            GoldUser goldUser = GetUser(user.Id);

            await ReplyAsync(Util.FormatText(user.Username, "**") + " has **" + goldUser.gold + "** vgold.");
        }

        [Command("chest")]
        public async Task OpenChest()
        {
            GoldUser goldUser = GetUser(Context.User.Id);
            int amount = Chest(goldUser);

            string reply = "";
            if (amount < 0)
            {
                reply = Util.FormatText(Context.User.Username, "**") + ", you have already opened a chest today.";
            }
            else
            {
                reply = Util.FormatText(Context.User.Username, "**") + ", you opened a chest and found **" + amount + "** gold coins!\n";
                reply += "You now have **" + goldUser.gold + "** gold coins.";
            }
            await ReplyAsync(reply);
        }

        [Command("give")]
        public async Task GiveGold(SocketGuildUser user, int amount)
        {
            if (user.Id == Context.User.Id && (user.Id != Program.owner || amount > 0))
            {
                await ReplyAsync("You cannot give gold to yourself.");
                return;
            }
            if (user.IsBot)
            {
                await ReplyAsync("You cannot give gold to a bot.");
                return;
            }

            GoldUser src = GetUser(Context.User.Id);
            GoldUser dst = GetUser(user.Id);

            if (amount < 0)
            {
                if (Context.User.Id == Program.owner)
                {
                    dst.AddGold(-amount);
                    await ReplyAsync(Util.FormatText(user.Username, "**") + " found **" + (-amount) + "** gold coins!");
                }
                else
                {
                    await ReplyAsync("Invalid amount.");
                }
                return;
            }

            if (amount > src.gold)
            {
                await ReplyAsync(Util.FormatText(Context.User.Username, "**") + ", you do not have enough gold (" + src.gold + ")!");
                return;
            }

            src.AddGold(-amount);
            dst.AddGold(amount);

            string reply = Util.FormatText(Context.User.Username, "**") + " gave **" + amount + "** gold coins to " + Util.FormatText(user.Username, "**") + "\n";
            reply += Util.FormatText(Context.User.Username, "**") + " (" + src.gold + ")\n";
            reply += Util.FormatText(user.Username, "**") + " (" + dst.gold + ")";

            await ReplyAsync(reply);
        }
    }
}
