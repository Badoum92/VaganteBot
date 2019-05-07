using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Game = VaganteBot.modules.games.Game;

namespace VaganteBot
{
    public class TicTacToe : Game
    {
        SocketGuildUser p1;
        SocketGuildUser p2;
        SocketGuildUser currentPlayer;

        char[] grid;
        // token empty = '-'
        // token p1 = 'X'
        // token p2 = 'O'
        char token;

        readonly IEmote[] reactions = new IEmote[]
        {
            new Emoji("↖"), new Emoji("⬆"), new Emoji("↗"),
            new Emoji("⬅"), new Emoji("⏺"), new Emoji("➡"),
            new Emoji("↙"), new Emoji("⬇"), new Emoji("↘")
        };

        public TicTacToe(SocketGuildUser p1, SocketGuildUser p2, ISocketMessageChannel channel)
        {
            this.p1 = p1;
            this.p2 = p2;

            currentPlayer = p1;
            token = 'X';
            grid = new char[9] 
            {
                '-', '-', '-',
                '-', '-', '-',
                '-', '-', '-'
            };

            Update(channel);
            games.Add(this);
        }

        private static int ReactionToIndex(SocketReaction reaction)
        {
            switch (reaction.Emote.Name)
            {
                case "↖":
                    return 0;
                case "⬆":
                    return 1;
                case "↗":
                    return 2;
                case "⬅":
                    return 3;
                case "⏺":
                    return 4;
                case "➡":
                    return 5;
                case "↙":
                    return 6;
                case "⬇":
                    return 7;
                case "↘":
                    return 8;
                default:
                    return -1;
            }
        }

        protected override void ReactionPlay(SocketReaction reaction)
        {
            // Check if the user is valid
            if (reaction.UserId != currentPlayer.Id)
                return;

            // Get the grid index from the reaction
            // Check if the reaction is valid and that the grid index is not taken yet
            int index = ReactionToIndex(reaction);
            if (index == -1 || grid[index] != '-')
                return;

            grid[index] = token;

            currentPlayer = currentPlayer.Id == p1.Id ? p2 : p1;
            token = token == 'O' ? 'X' : 'O';
            CheckWin();
        }

        private int CheckLines()
        {
            for (int l = 0; l < 3; ++l)
            {
                if (grid[l * 3] != '-' && grid[l * 3] == grid[l * 3 + 1] && grid[l * 3 + 1] == grid[l * 3 + 2])
                    return grid[l * 3] == 'X' ? 1 : 2;
            }
            return 0;
        }

        private int CheckCols()
        {
            for (int c = 0; c < 3; ++c)
            {
                if (grid[c] != '-' && grid[c] == grid[c + 3] && grid[c + 3] == grid[c + 6])
                    return grid[c] == 'X' ? 1 : 2;
            }
            return 0;
        }

        private int CheckDiags()
        {
            if (grid[0] != '-' && grid[0] == grid[4] && grid[4] == grid[8])
                return grid[0] == 'X' ? 1 : 2;
            if (grid[2] != '-' && grid[2] == grid[4] && grid[4] == grid[6])
                return grid[2] == 'X' ? 1 : 2;
            return 0;
        }

        private void CheckWin()
        {
            // winner == 0 -> nobody won
            // winner == 1 -> p1 won
            // winner == 2 -> p2 won
            int winner = Math.Max(CheckDiags(), Math.Max(CheckLines(), CheckCols()));
            if (winner > 0)
            {
                if (winner == 1)
                    Update(null, Format.FormatText(p1.Username, "**") + " won!");
                else
                    Update(null, Format.FormatText(p2.Username, "**") + " won!");
                games.Remove(this);
                return;
            }

            // Check if the grid is full (draw)
            if (grid.Any(c => c == '-'))
            {
                Update();
                return;
            }

            Update(null, "It's a draw.");
            games.Remove(this);
        }
        
        private static string EmoteFromChar(char c)
        {
            switch (c)
            {
                case '-':
                    return "⬜";
                case 'X':
                    return "🔷";
                case 'O':
                    return "🔴";
                default:
                    return "";
            }
        }

        public override string ToString()
        {
            string str = $"{Format.FormatText(p1.Username, "**")} VS {Format.FormatText(p2.Username, "**")}\n" +
                         $"{EmoteFromChar(grid[0])}{EmoteFromChar(grid[1])}{EmoteFromChar(grid[2])}\n" +
                         $"{EmoteFromChar(grid[3])}{EmoteFromChar(grid[4])}{EmoteFromChar(grid[5])}\n" +
                         $"{EmoteFromChar(grid[6])}{EmoteFromChar(grid[7])}{EmoteFromChar(grid[8])}";
            return str;
        }

        private async void Update(ISocketMessageChannel channel = null, string suffix = "")
        {
            if (suffix == "")
            {
                suffix = Format.FormatText(currentPlayer.Username, "**") + "'s turn.";
            }

            if (channel == null && msg == null)
            {
                return;
            }
            else if (msg == null)
            {
                // Message hasn't been sent yet
                // Send it and assign it to the msg variable
                msg = await channel.SendMessageAsync(ToString() + "\n" + suffix);
                await msg.AddReactionsAsync(reactions);
            }
            else
            {
                // Message has already been sent, just update it
                await msg.ModifyAsync(m => m.Content = ToString() + "\n" + suffix);
            }
        }
    }
}
