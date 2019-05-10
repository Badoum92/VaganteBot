using System;
using System.Collections.Generic;
using System.Numerics;

namespace VaganteBot.modules.games
{
    public class Card
    {
        public enum Suit { SPADES, DIAMONDS, CLUBS, HEARTS };

        public Suit suit { get; private set; }
        public int val { get; private set; }

        public Card(Suit suit_, int val_)
        {
            suit = suit_;
            val = val_;
        }

        public override string ToString()
        {
            return ValToEmote(val) + SuitToEmote(suit);
        }

        private static string SuitToEmote(Suit s)
        {
            switch (s)
            {
                case Suit.SPADES:
                    return ":spades:";
                case Suit.DIAMONDS:
                    return ":diamonds:";
                case Suit.CLUBS:
                    return ":clubs:";
                case Suit.HEARTS:
                    return ":hearts:";
                default:
                    return "";
            }
        }
        
        private static string ValToEmote(int n)
        {
            switch (n)
            {
                case 1:
                    return ":regional_indicator_a:";
                case 2:
                    return ":two:";
                case 3:
                    return ":three:";
                case 4:
                    return ":four:";
                case 5:
                    return ":five:";
                case 6:
                    return ":six:";
                case 7:
                    return ":seven:";
                case 8:
                    return ":eight:";
                case 9:
                    return ":nine:";
                case 10:
                    return ":keycap_ten:";
                case 11:
                    return ":regional_indicator_j:";
                case 12:
                    return ":regional_indicator_q:";
                case 13:
                    return ":regional_indicator_k:";
                case 14:
                    return ":regional_indicator_a:";
                default:
                    return "";
            }
        }

        public static List<Card> NewDeck(bool shuffle = true)
        {
            var deck = new List<Card>();
            for (int i = 1; i <= 13; ++i)
            {
                deck.Add(new Card(Suit.SPADES, i));
                deck.Add(new Card(Suit.DIAMONDS, i));
                deck.Add(new Card(Suit.CLUBS, i));
                deck.Add(new Card(Suit.HEARTS, i));
            }

            if (shuffle)
            {
                var count = deck.Count;
                var last = count - 1;
                var rand = new Random();
                for (var i = 0; i < last; ++i) 
                {
                    var r = rand.Next(i, count);
                    var tmp = deck[i];
                    deck[i] = deck[r];
                    deck[r] = tmp;
                }
            }
            
            return deck;
        }
    }
}