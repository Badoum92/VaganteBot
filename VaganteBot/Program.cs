using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Game = VaganteBot.modules.games.Game;

namespace VaganteBot
{
    class Program
    {
        public const ulong owner = 209690783580815360;

        DiscordSocketClient client = null;
        CommandService commands = null;
        IServiceProvider services = null;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            client.Log += Log;
            client.ReactionAdded += OnReactionAdded;

            var token = File.ReadLines("data/token").First();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await client.SetGameAsync("v!help");

            await RegisterCommand();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task RegisterCommand()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        private async Task ExecuteCommand(SocketUserMessage msg, int argPos)
        {
            Console.WriteLine("[" + DateTime.Now + "] #" + msg.Channel.Name + " | " + msg.Author.Username + ": " + msg.Content);
            var context = new SocketCommandContext(client, msg);
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                Console.WriteLine(result.ErrorReason);
        }

        private async Task HandleCommand(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message is null || message.Author.IsBot)
                return;

            var argPos = 0;

            if (message.HasStringPrefix("v!!", ref argPos))
            {
                await ExecuteCommand(message, argPos);
                await message.DeleteAsync();
            }
            else if (message.HasStringPrefix("v!", ref argPos))
            {
                await ExecuteCommand(message, argPos);
            }
        }

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            Game.HandleReaction(reaction);
            await Task.CompletedTask;
        }
    }
}