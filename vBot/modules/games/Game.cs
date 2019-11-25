using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace vBot.modules.games
{
    public abstract class Game
    {
        // Main game message
        protected IUserMessage msg = null;

        // All active games
        protected static List<Game> games = new List<Game>();

        // What should happen when a reaction is added to msg
        protected abstract void ReactionPlay(SocketReaction reaction);

        // Send the reaction to the game matching the message
        public static void HandleReaction(SocketReaction reaction)
        {
            var found = games.Where(g => g.msg.Id == reaction.MessageId);
            if (found.Count() != 1)
                return;
            found.First().ReactionPlay(reaction);
        }
    }
}
