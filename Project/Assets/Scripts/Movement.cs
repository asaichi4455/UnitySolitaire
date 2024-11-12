using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Solitaire
{
    public class Movement
    {
        private static readonly int WasteOffsetY = 16;
        private static readonly int PileOffsetY = 16;
        private static readonly int MinPileOffsetY = 6;
        private static readonly int DraggingCardSortingOrder = 100;

        private List<CardInfo> _cards;
        private CardCoordinates _coordinates;
        private Bounds[] _pileDropBounds;
        private Dictionary<CardInfo, bool> _isAnimate = new Dictionary<CardInfo, bool>();

        public bool IsAnimate
        {
            get { return _isAnimate.Any(pair => pair.Value); }
        }

        public void Init(List<CardInfo> cards, CardCoordinates cardCoordinates, Bounds[] pileDropBounds)
        {
            _cards = cards;
            _coordinates = cardCoordinates;
            _pileDropBounds = pileDropBounds;
            foreach (CardInfo card in cards)
            {
                card.OnBeginDrag += OnBeginDrag;
                card.OnDrag += OnDrag;
                _isAnimate.Add(card, false);
            }
        }

        public void PrepareStock()
        {
            foreach (CardInfo card in _cards)
            {
                card.transform.position = _coordinates.Stock;
            }
        }

        public void MoveStockToWaste(CardInfo card, int order)
        {
            var numWaste = _cards.Where(c => c.CardType == CardType.Waste).Count();
            var pos = _coordinates.Waste;
            pos.y -= (order * WasteOffsetY);
            card.IsFacedown = false;
            card.SortingOrder = numWaste + order;
            var _ = AnimateAsync(card, pos);
        }

        public void MoveWasteToStock()
        {
            var order = 0;
            foreach (CardInfo card in _cards)
            {
                if (card.CardType == CardType.Stock)
                {
                    card.IsFacedown = true;
                    card.SortingOrder = order++;
                    var _ = AnimateAsync(card, _coordinates.Stock);
                }
            }
        }

        public void MoveWaste(CardInfo card, int order)
        {
            var pos = _coordinates.Waste;
            pos.y -= (order * WasteOffsetY);
            var _ = AnimateAsync(card, pos);
        }

        public void MoveWasteFull()
        {
            var waste = _cards.Where(c => c.CardType == CardType.Waste).Take(Game.MaxWastes).Reverse().ToList();
            var order = 0;
            foreach (var w in waste)
            {
                var pos = _coordinates.Waste;
                pos.y -= (order++ * WasteOffsetY);
                var _ = AnimateAsync(w, pos);
            }
        }

        public async UniTask MoveToPile(CardInfo card, int pileIndex, int order)
        {
            // –‡”‚É‰ž‚¶‚ÄŠÔŠu‚ð’²®
            var pileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex).OrderBy(c => c.Order).ToList();
            var offsetFacedown = PileOffsetY;
            var offsetFaceup = PileOffsetY;
            var cardHeight = card.Size.y;
            var heightOver = cardHeight + (pileCards.Count - 1) * PileOffsetY - _pileDropBounds[pileIndex].size.y;
            if (heightOver > 0)
            {
                var numFacedown = pileCards.Count(c => c.IsFacedown);
                var numFaceUp = pileCards.Count - numFacedown;
                offsetFacedown -= Mathf.CeilToInt(heightOver / numFacedown);
                offsetFacedown = Mathf.Clamp(offsetFacedown, MinPileOffsetY, PileOffsetY);
                heightOver = cardHeight + numFacedown * offsetFacedown + (numFaceUp - 1) * offsetFaceup - _pileDropBounds[pileIndex].size.y;
                if (heightOver > 0)
                {
                    offsetFaceup -= Mathf.CeilToInt(heightOver / (numFaceUp - 1));
                    offsetFaceup = Mathf.Clamp(offsetFaceup, MinPileOffsetY, PileOffsetY);
                }
            }

            var pos = _coordinates.Piles[pileIndex];
            foreach (var c in pileCards)
            {
                if (c.Order == order)
                    break;
                pos.y -= (c.IsFacedown ? offsetFacedown : offsetFaceup);
            }

            card.SortingOrder = DraggingCardSortingOrder + order;
            await  AnimateAsync(card, pos);
            card.SortingOrder = order;
        }

        public void MoveToFoundation(CardInfo card, int order)
        {
            var pos = _coordinates.Foundations[(int)card.Suit];
            card.SortingOrder = DraggingCardSortingOrder + order;
            var _ = AnimateAsync(card, pos);
        }

        public async UniTask MoveToPrev(CardInfo card)
        {
            var task = new List<UniTask>
            {
                AnimateAsync(card, card.PrevPosition)
            };
            List<CardInfo> connectedCards = null;

            if (card.CardType == CardType.Pile)
            {
                connectedCards = Utility.GetConnectedCards(_cards, card);
                for (var i = 0; i < connectedCards.Count(); ++i)
                {
                    task.Add(AnimateAsync(connectedCards[i], connectedCards[i].PrevPosition));
                }
            }

            await UniTask.WhenAll(task);

            card.SortingOrder = card.PrevSortingOrder;
            if (card.CardType == CardType.Pile)
            {
                for (var i = 0; i < connectedCards.Count(); ++i)
                {
                    connectedCards[i].SortingOrder = connectedCards[i].PrevSortingOrder;
                }
            }
        }

        public async UniTask MoveToPrev(List<CardInfo> cards)
        {
            for (var i = 0; i < cards.Count(); ++i)
            {
                await AnimateAsync(cards[i], cards[i].PrevPosition);
                cards[i].SortingOrder = cards[i].PrevSortingOrder;
            }
        }

        private void OnBeginDrag(CardInfo card, Vector2 pos)
        {
            if (IsAnimate)
                return;

            card.PrevPosition = card.transform.position;
            card.PrevSortingOrder = card.SortingOrder;
            card.SortingOrder = DraggingCardSortingOrder + (int)card.Number;

            if (card.CardType == CardType.Pile)
            {
                var cards = Utility.GetConnectedCards(_cards, card);
                for (var i = 0; i < cards.Count(); ++i)
                {
                    cards[i].PrevPosition = cards[i].transform.position;
                    cards[i].PrevSortingOrder = cards[i].SortingOrder;
                    cards[i].SortingOrder = DraggingCardSortingOrder + (int)card.Number + i + 1;
                }
            }
        }

        private void OnDrag(CardInfo card, Vector2 pos)
        {
            if (IsAnimate)
                return;

            card.transform.position = pos;

            if (card.CardType == CardType.Pile)
            {
                var cards = Utility.GetConnectedCards(_cards, card);
                for (var i = 0; i < cards.Count(); ++i)
                {
                    var p = card.transform.position;
                    p.y -= ((cards[i].Order - card.Order) * WasteOffsetY);
                    cards[i].transform.position = p;
                }
            }
        }

        private async UniTask AnimateAsync(CardInfo card, Vector2 dst)
        {
            _isAnimate[card] = true;

            while (true)
            {
                await UniTask.Delay(Mathf.CeilToInt(Time.deltaTime * 1000));
                var pos = card.transform.position;
                var x = Mathf.Lerp(pos.x, dst.x, 0.1f);
                var y = Mathf.Lerp(pos.y, dst.y, 0.1f);
                if (Mathf.Abs(dst.x - x) < 2f) x = dst.x;
                if (Mathf.Abs(dst.y - y) < 2f) y = dst.y;
                card.transform.position = new Vector2(x, y);
                if (Mathf.Approximately(x, dst.x) && Mathf.Approximately(y, dst.y))
                {
                    _isAnimate[card] = false;
                    break;
                }
            }
        }
    }
}
