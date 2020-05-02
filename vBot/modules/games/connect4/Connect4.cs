using System;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace vBot.modules.games.connect4
{
    public class Connect4 : Game
    {
        SocketGuildUser p1;
        SocketGuildUser p2;
        SocketGuildUser currentPlayer;
        int currentTurn = 0;

        private const int width = 7;
        private const int height = 6;
        char[,] grid = new char[width, height];

        readonly IEmote[] reactions =
        {
            new Emoji("🇦"), new Emoji("🇧"), new Emoji("🇨"), new Emoji("🇩"), new Emoji("🇪"), new Emoji("🇫"), new Emoji("🇬"), new Emoji("🛑")
        };

        public Connect4(SocketGuildUser p1,  SocketGuildUser p2, ISocketMessageChannel channel)
        {
            this.p1 = p1;
            this.p2 = p2;
            currentPlayer = p1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = '-';
                }
            }

            Update(channel);
            games.Add(this);
        }

        protected override void ReactionPlay(SocketReaction reaction)
        {
            if (reaction.UserId != currentPlayer.Id)
                return;

            if (reaction.Emote.Name == "🛑")
            {
                games.Remove(this);
                string w = currentPlayer.Id == p1.Id ?  p2.Username : p1.Username;
                Update(null, $"{Discord.Format.Bold(w)} won!");
                return;
            }

            int col = ColumnFromReaction(reaction);
            if (grid[col, height - 1] != '-')
                return;

            int line = height - 1;
            for (; line >= 0; line--)
            {
                if (grid[col, line] != '-')
                    break;
            }

            grid[col, ++line] = GetCurrentToken();

            currentTurn++;
            int win = CheckWin(col, line);
            if (win == 0)
            {
                if (currentTurn == 42)
                {
                    Update(null, "It's a draw!");
                    games.Remove(this);
                }
                else
                {
                    currentPlayer = currentPlayer == p1 ? p2 : p1;
                    Update();
                }
                return;
            }

            games.Remove(this);
            SocketGuildUser winner = win == 1 ? p1 : p2;
            Update(null, $"{Discord.Format.Bold(winner.Mention)} won!");
        }

        private int CheckWin(int col, int line)
        {
            int cCol = CheckCol(col);
            int cLine = CheckLine(line);
            int cBld = CheckBottomLeftDiag(col, line);
            int cBrd = CheckBottomRightDiag(col, line);

            if (cCol != 0)
                return cCol;
            if (cLine != 0)
                return cLine;
            if (cBld != 0)
                return cBld;
            if (cBrd != 0)
                return cBrd;

            return 0;
        }

        private int CheckCol(int col)
        {
            int line = 0;
            int count = 0;
            int cur = '-';

            for (; line < height; line++)
            {
                UpdateCount(col, line, ref count, ref cur);
                if (count == 4)
                    break;
            }

            if (cur == '-' || count < 4)
                return 0;
            return cur == 'X' ? 1 : 2;
        }

        private int CheckLine(int line)
        {
            int col = 0;
            int count = 0;
            int cur = '-';

            for (; col < width; col++)
            {
                UpdateCount(col, line, ref count, ref cur);
                if (count == 4)
                    break;
            }

            if (cur == '-' || count < 4)
                return 0;
            return cur == 'X' ? 1 : 2;
        }

        private int CheckBottomLeftDiag(int col, int line)
        {
            while (col > 0 && line > 0)
            {
                col--;
                line--;
            }

            int count = 0;
            int cur = '-';

            while (col < width && line < height)
            {
                UpdateCount(col, line, ref count, ref cur);
                if (count == 4)
                    break;
                col++;
                line++;
            }

            if (cur == '-' || count < 4)
                return 0;
            return cur == 'X' ? 1 : 2;
        }

        private int CheckBottomRightDiag(int col, int line)
        {
            while (col < width - 1 && line > 0)
            {
                col++;
                line--;
            }

            int count = 0;
            int cur = '-';

            while (col >= 0 && line < height)
            {
                UpdateCount(col, line, ref count, ref cur);
                if (count == 4)
                    break;
                col--;
                line++;
            }

            if (cur == '-' || count < 4)
                return 0;
            return cur == 'X' ? 1 : 2;
        }

        private void UpdateCount(int col, int line, ref int count, ref int cur)
        {
            if (grid[col, line] == '-')
            {
                count = 0;
                cur = '-';
            }
            else
            {
                if (cur == grid[col, line])
                {
                    count++;
                }
                else
                {
                    count = 1;
                    cur = grid[col, line];
                }
            }
        }

        private char GetCurrentToken()
        {
            return currentPlayer.Id == p1.Id ? 'X' : 'O';
        }
        
        private int ColumnFromReaction(SocketReaction reaction)
        {
            switch (reaction.Emote.Name)
            {
                case "🇦":
                    return 0;
                case "🇧":
                    return 1;
                case "🇨":
                    return 2;
                case "🇩":
                    return 3;
                case "🇪":
                    return 4;
                case "🇫":
                    return 5;
                case "🇬":
                    return 6;
            }
            return -1;
        }

        private string EmoteFromChar(char c)
        {
            switch (c)
            {
                case '-':
                    return "⚪";
                case 'X':
                    return "🔴";
                case 'O':
                    return "🔵";
            }
            return "";
        }

        public override string ToString()
        {
            string s = "";
            for (int y = height - 1; y >= 0; y--)
            {
                s += EmoteFromChar(grid[0, y]);
                for (int x = 1; x < width; x++)
                {
                    s += "       " + EmoteFromChar(grid[x, y]);
                }
                s += "\n\n";
            }
            return s;
        }

        private async void Update(ISocketMessageChannel channel = null, string suffix = "")
        {
            if (suffix == "")
            {
                suffix = $"{currentPlayer.Mention}'s turn.";
            }

            if (channel == null && msg == null)
            {
            }
            else if (msg == null)
            {
                msg = await channel.SendMessageAsync(ToString() + "\n" + suffix);
                await msg.AddReactionsAsync(reactions);
            }
            else
            {
                await msg.ModifyAsync(m => m.Content = ToString() + "\n" + suffix);
            }
        }
    }
}
