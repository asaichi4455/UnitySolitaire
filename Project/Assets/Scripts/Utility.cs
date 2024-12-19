using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
    public static class Utility
    {
        public static int GetNumTurnToWaste(Difficulty difficulty)
        {
            var ret = 1;
            switch (difficulty)
            {
                case Difficulty.Easy: ret = 1; break;
                case Difficulty.Hard: ret = 3; break;
                default:                       break;
            }
            return ret;
        }

        public static bool TryMoveToPile(List<CardInfo> cards, CardInfo card, out int pileIndex)
        {
            var ret = false;
            for (pileIndex = 0; pileIndex < Game.NumPiles; ++pileIndex)
            {
                if (CanMoveToPile(cards, card, pileIndex))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public static bool CanMoveToPile(List<CardInfo> cards, CardInfo card, int pileIndex)
        {
            var ret = false;
            var c = cards
                .Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex)
                .OrderBy(c => c.Order)
                .LastOrDefault();

            if (c == null)
            {
                if (card.Number == Number.K)
                    ret = true;
                return ret;
            }

            if ((c.Suit == Suit.Heart || c.Suit == Suit.Diamond) && (card.Suit == Suit.Club || card.Suit == Suit.Spade))
            {
                if (c.Number == card.Number + 1)
                    ret = true;
            }
            else if ((c.Suit == Suit.Club || c.Suit == Suit.Spade) && (card.Suit == Suit.Heart || card.Suit == Suit.Diamond))
            {
                if (c.Number == card.Number + 1)
                    ret = true;
            }
            return ret;
        }

        public static bool TryMoveToFoundation(List<CardInfo> cards, CardInfo card, out Suit suit)
        {
            var ret = false;
            for (suit = 0; suit < Suit.Num; ++suit)
            {
                if (CanMoveToFoundation(cards, card, suit))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public static bool CanMoveToFoundation(List<CardInfo> cards, CardInfo card, Suit suit)
        {
            var ret = false;

            if (card.Suit != suit)
                return ret;

            var c = cards
                .Where(c => c.CardType == CardType.Foundation && c.Suit == suit)
                .OrderBy(c => c.Order)
                .LastOrDefault();

            if (c == null)
            {
                if (card.Number == Number.A)
                    ret = true;
                return ret;
            }

            if ((card.Suit == c.Suit) && (card.Number == c.Number + 1))
                ret = true;
            return ret;
        }

        public static List<CardInfo> GetConnectedCards(List<CardInfo> cards, CardInfo card)
        {
            if (card.CardType != CardType.Pile)
                return null;

            return cards
                .Where(c => c.CardType == CardType.Pile && c.PileIndex == card.PileIndex && c.Order > card.Order)
                .OrderBy(c => c.Order)
                .ToList();
        }

        public static bool IsGameClear(List<CardInfo> cards, int numTurnToWaste)
        {
            // 裏の場札がある
            if (cards.Any(c => c.CardType == CardType.Pile && c.IsFacedown))
                return false;

            // 山札が残っている
            if (cards.Any(c => c.CardType == CardType.Stock))
                return false;

            // 山札からめくったカードの中で操作できないカードがある
            var numWaste = cards.Where(c => c.CardType == CardType.Waste).Count();
            if (numWaste > Game.MaxWastes)
                return false;
            if (numTurnToWaste > 1 && numWaste > 1)
                return false;

            return true;
        }
    }
}
