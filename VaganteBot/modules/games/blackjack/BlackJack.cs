using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace VaganteBot.modules.games.blackjack
{
    public class BlackJack : Game
    {
        private SocketGuildUser player;
        private int bet;
        private List<Card> deck;
        private List<Card> player_cards;
        private List<Card> bot_cards;

        private bool reveal;

        readonly IEmote[] reactions = 
        {
            new Emoji("💳"), new Emoji("🛑")
        };

        public BlackJack(SocketGuildUser player_, int bet_, ISocketMessageChannel channel)
        {
            player = player_;
            bet = bet_;
            deck = Card.NewDeck();
            player_cards = new List<Card>();
            bot_cards = new List<Card>();
            reveal = false;

            Draw(player_cards);
            Draw(player_cards);
            Draw(bot_cards);
            Draw(bot_cards);

            int score = GetScore(player_cards);
            string suffix = "Your score: **" + score + "**\nBot's score: **???**";
            Update(channel, suffix);

            if (score == 21)
            {
                Update(null, suffix + "\nYou won! **+" + 1.5f * bet + "**");
                return;
            }
            
            games.Add(this);
        }

        private void Draw(List<Card> cards)
        {
            cards.Add(deck[0]);
            deck.RemoveAt(0);
        }

        private void Stand()
        {
            while (GetScore(bot_cards) <= 16)
            {
                Draw(bot_cards);
            }

            reveal = true;
        }

        private void CheckWin()
        {
            int playerScore = GetScore(player_cards);
            int botScore = GetScore(bot_cards);

            if (!reveal)
            {
                string suffix = "Your score: **" + playerScore + "**\nBot's score: **???**";
                if (playerScore > 21)
                {
                    Update(null, suffix + "\nYou lost! **-" + bet + "**");
                    games.Remove(this);
                }
                else
                {
                    Update(null, suffix);
                }
            }
            else
            {
                string suffix = "Your score: **" + playerScore + "**\nBot's score: **" + botScore + "**";
                if (botScore > 21 || playerScore > botScore)
                    suffix += "\nYou won! **+" + 2 * bet + "**";
                else if (botScore == playerScore)
                    suffix += "\nIt's a draw! **+0**";
                else
                    suffix += "\nYou lost! **-" + bet + "**";
                Update(null, suffix);
                games.Remove(this);
            }
        }

        private int GetScore(List<Card> cards)
        {
            int score = 0;
            int aces = 0;
            foreach (var card in cards)
            {
                score += card.BlackJackVal();
                if (card.val == 1)
                    aces++;
            }

            for (; aces > 0; aces--)
            {
                if (score <= 21)
                    break;
                score -= 10;
            }

            return score;
        }

        private async void Update(ISocketMessageChannel channel, string suffix = "")
        {
            if (channel != null)
            {
                msg = await channel.SendMessageAsync(ToString() + "\n" + suffix);
                await msg.AddReactionsAsync(reactions);
            }
            else
            {
                await msg.ModifyAsync(m => m.Content = ToString() + "\n" + suffix);
            }
        }

        protected override void ReactionPlay(SocketReaction reaction)
        {
            if (reaction.UserId != player.Id)
                return;

            if (reaction.Emote.Name == "💳")
            {
                Draw(player_cards);
                CheckWin();
            }
            else if (reaction.Emote.Name == "🛑")
            {
                Stand();
                CheckWin();
            }
        }

        public override string ToString()
        {
            string str = "<@" + player.Id + ">'s cards: :white_small_square:  ";
            foreach (var card in player_cards)
            {
                str += card + "  :white_small_square:";
            }

            str += "\n**Bot**'s cards: :white_small_square:  ";

            if (reveal)
            {
                foreach (var card in bot_cards)
                {
                    str += card + "  :white_small_square:";
                }
            }
            else
            {
                str += bot_cards[0] + "  :white_small_square:  :question::question:  :white_small_square:";
            }

            return str;
        }
    }
}