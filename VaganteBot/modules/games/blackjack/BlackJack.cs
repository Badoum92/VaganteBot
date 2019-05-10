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

        readonly IEmote[] reactions = new IEmote[]
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
            string suffix = "Your score: **" + score + "**\nBot's score: ???";
            
            Update(channel, suffix);
            games.Add(this);
        }

        private void Draw(List<Card> cards)
        {
            cards.Add(deck[0]);
            deck.RemoveAt(0);
        }

        private int GetScore(List<Card> cards)
        {
            int score = 0;
            foreach (var card in cards)
            {
                if (card.val == 1)
                {
                    if (score + 11 > 21)
                        score += 1;
                    else
                        score += 11;
                }
                else
                {
                    score += card.val;
                }
            }

            return score;
        }
        
        private void Update(ISocketMessageChannel channel, string suffix = "")
        {
            channel.SendMessageAsync(ToString() + "\n" + suffix);
        }

        protected override void ReactionPlay(SocketReaction reaction)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            string str = "<@" + player.Id + ">'s cards: :white_small_square:  ";
            foreach (var card in player_cards)
            {
                str += card.ToString() + "  :white_small_square:";
            }

            str += "\n**Bot**'s cards: :white_small_square:  ";

            if (reveal)
            {
                foreach (var card in bot_cards)
                {
                    str += card.ToString() + "  :white_small_square:";
                }
            }
            else
            {
                str += bot_cards[0].ToString() + "  :white_small_square:  :question::question:  :white_small_square:";
            }

            return str;
        }
    }
}