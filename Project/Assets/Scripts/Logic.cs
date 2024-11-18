using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Solitaire
{
    public class Logic
    {
        public event Action<CardInfo, int> OnMoveStockToWaste;
        public event Action OnMoveWasteToStock;
        public event Action<CardInfo, int> OnMoveWaste;
        public event Action OnMoveWasteFull;
        public event Action<CardInfo, int, int> OnMoveToPile;
        public event Action<CardInfo, int, bool> OnMoveToFoundation;
        public event Action<CardInfo> OnMoveToPrev;
        public event Action<CardMoveStep> OnMoveOneStep;
        public event Action OnGameClear;
        public event Func<bool> IsEnableMove;

        private List<CardInfo> _cards;
        private CardDropBounds _dropBounds;
        private int _numTurnToWaste = 1;

        public int NumTurnToWaste
        {
            set { _numTurnToWaste = value; }
        }

        public void Init(List<CardInfo> cards, CardDropBounds bounds)
        {
            _cards = cards;
            _dropBounds = bounds;
            foreach (CardInfo card in cards)
            {
                card.OnClick += OnClick;
                card.OnBeginDrag += OnBeginDrag;
                card.OnEndDrag += OnEndDrag;
            }
        }

        public void DealCards(int numPiles)
        {
            var index = 0;
            for (var p = 0; p < numPiles; ++p)
            {
                for (var o = 0; o < p + 1; ++o)
                {
                    var card = _cards[index++];
                    card.CardType = CardType.Pile;
                    card.PileIndex = p;
                    card.Order = o;
                    card.IsFacedown = (o == p ? false : true);
                    card.Clickable = (o == p ? true : false);
                    OnMoveToPile?.Invoke(card, p, o);
                }
            }
        }

        public void SetCardsGameClear()
        {
            foreach (var c in _cards)
            {
                if (c.CardType != CardType.Foundation)
                {
                    c.CardType = CardType.Foundation;
                    c.Order = (int)c.Number;
                    OnMoveToFoundation(c, c.Order, true);
                }
            }
        }

        public void OnClickStock()
        {
            var isEnable = IsEnableMove?.Invoke();
            if (isEnable != null && !isEnable.Value)
                return;

            // �R�D���Փx�ɉ�����1����3���߂���
            // �R�D���Ȃ��Ȃ�΂߂������J�[�h�����ׂĎR�D�ɂ��ǂ�
            var numStock = _cards.Where(c => c.CardType == CardType.Stock).Count();
            if (numStock > 0)
            {
                var wasteCards = _cards.Where(c => c.CardType == CardType.Waste).ToList();
                var wasteTop = wasteCards
                    .Take(Game.MaxWastes)
                    .Reverse()
                    .ToList();

                var turnCards = _cards
                    .Where(c => c.CardType == CardType.Stock)
                    .TakeLast(_numTurnToWaste)
                    .Reverse()
                    .ToList();

                // �������J�[�h���߂���
                for (int i = 0; i < turnCards.Count; ++i)
                {
                    turnCards[i].CardType = CardType.Waste;
                    turnCards[i].Clickable = false;
                    var order = Mathf.Clamp(wasteCards.Count, 0, Game.MaxWastes);
                    if (order + turnCards.Count - 1 > Game.MaxWastes - 1)
                    {
                        order -= (order + turnCards.Count - 1 - (Game.MaxWastes - 1));
                    }
                    order += i;
                    turnCards[i].Order = order;
                }
                for (int i = 0; i < turnCards.Count; ++i)
                {
                    OnMoveStockToWaste?.Invoke(turnCards[i], turnCards[i].Order);
                }

                // �߂����Ă����J�[�h�����Ɉړ�
                // ��O�̃J�[�h�̂ݑ���\�Ƃ���
                var numWasteTop = wasteTop.Count;
                var numTurnCards = turnCards.Count;
                if (numWasteTop + numTurnCards > Game.MaxWastes)
                {
                    for (var i = 0; i < wasteTop.Count; ++i)
                    {
                        var order = Mathf.Clamp(i - (numWasteTop + numTurnCards - Game.MaxWastes), 0, Game.MaxWastes - 1);
                        wasteTop[i].Order = order;
                        wasteTop[i].Clickable = false;
                        OnMoveWaste?.Invoke(wasteTop[i], order);
                    }
                }
                _cards.Where(c => c.CardType == CardType.Waste).First().Clickable = true;
                OnMoveOneStep?.Invoke(CardMoveStep.StockToWaste);
            }
            else
            {
                // ���ׂĎR�D�ɖ߂�
                var wasteCards = _cards.Where(c => c.CardType == CardType.Waste).ToList();
                if (wasteCards.Count > 0)
                {
                    for (var i = 0; i < wasteCards.Count(); ++i)
                    {
                        wasteCards[i].CardType = CardType.Stock;
                        wasteCards[i].Clickable = false;
                    }
                    OnMoveWasteToStock?.Invoke();
                    OnMoveOneStep?.Invoke(CardMoveStep.WasteToStock);
                }
            }
        }

        private void WasteToFoundation(CardInfo card, Suit suit)
        {
            // �g�D�Ɉړ�
            card.Order = (int)card.Number;
            card.CardType = CardType.Foundation;
            OnMoveToFoundation?.Invoke(card, card.Order, false);

            if (Utility.IsGameClear(_cards, _numTurnToWaste))
            {
                OnGameClear?.Invoke();
            }
            else
            {
                // ���łɂ߂������J�[�h���[
                var waste = _cards.Where(c => c.CardType == CardType.Waste).Take(Game.MaxWastes).Reverse().ToList();
                if (waste.Count > 0)
                {
                    var order = 0;
                    foreach (var w in waste)
                    {
                        w.Order = order++;
                    }
                    waste.Last().Clickable = true;
                    OnMoveWasteFull?.Invoke();
                }
            }

            OnMoveOneStep?.Invoke(CardMoveStep.WasteToFoundation);
        }

        private void WasteToPile(CardInfo card, int pileIndex)
        {
            // ��D�Ɉړ�
            card.Order = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex).Count();
            card.CardType = CardType.Pile;
            card.PileIndex = pileIndex;

            // �ړ���̏�D�̈ʒu���X�V
            var pileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex);
            foreach (var c in pileCards)
            {
                OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
            }

            // ���łɂ߂������J�[�h���[
            var waste = _cards.Where(c => c.CardType == CardType.Waste).Take(Game.MaxWastes).Reverse().ToList();
            if (waste.Count > 0)
            {
                var order = 0;
                foreach (var w in waste)
                {
                    w.Order = order++;
                }
                waste.Last().Clickable = true;
                OnMoveWasteFull?.Invoke();
            }

            OnMoveOneStep?.Invoke(CardMoveStep.WasteToPile);
        }

        private void PileToFoundation(CardInfo card, Suit suit)
        {
            var srcPileIndex = card.PileIndex;

            // �g�D�Ɉړ�
            card.Order = (int)card.Number;
            card.CardType = CardType.Foundation;
            OnMoveToFoundation?.Invoke(card, card.Order, false);

            if (Utility.IsGameClear(_cards, _numTurnToWaste))
            {
                OnGameClear?.Invoke();
            }
            else
            {
                // �ړ����̏�D�̗�̐擪�J�[�h���߂���
                var srcPileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == srcPileIndex).OrderBy(c => c.Order);
                if (srcPileCards.Count() > 0)
                {
                    foreach (var c in srcPileCards)
                    {
                        OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
                    }
                    var bottomCard = srcPileCards.Last();
                    if (bottomCard.IsFacedown)
                    {
                        bottomCard.Clickable = true;
                        bottomCard.IsFacedown = false;
                        OnMoveOneStep?.Invoke(CardMoveStep.FaceupPile);
                    }
                }
            }

            OnMoveOneStep?.Invoke(CardMoveStep.PileToFoundation);
        }

        private void FoundationToPile(CardInfo card, int pileIndex)
        {
            // ��D�Ɉړ�
            card.Order = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex).Count();
            card.CardType = CardType.Pile;
            card.PileIndex = pileIndex;

            // �ړ���̏�D�̈ʒu���X�V
            var pileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex);
            foreach (var c in pileCards)
            {
                OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
            }

            OnMoveOneStep?.Invoke(CardMoveStep.FoundationToPile);
        }

        private void PileToPile(CardInfo card, int pileIndex, List<CardInfo> connectedCards)
        {
            var srcPileIndex = card.PileIndex;

            // ��D�Ɉړ�
            card.Order = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex).Count();
            card.CardType = CardType.Pile;
            card.PileIndex = pileIndex;

            // �A�������J�[�h�𓯎��Ɉړ�
            for (var i = 0; i < connectedCards.Count(); ++i)
            {
                connectedCards[i].Order = card.Order + i + 1;
                connectedCards[i].CardType = CardType.Pile;
                connectedCards[i].PileIndex = pileIndex;
            }

            // �ړ���̏�D�̈ʒu���X�V
            var pileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex);
            foreach (var c in pileCards)
            {
                OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
            }

            // �ړ����̏�D�̗�̐擪�J�[�h���߂���
            var srcPileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == srcPileIndex).OrderBy(c => c.Order);
            if (srcPileCards.Count() > 0)
            {
                foreach (var c in srcPileCards)
                {
                    OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
                }
                var bottomCard = srcPileCards.Last();
                if (bottomCard.IsFacedown)
                {
                    bottomCard.Clickable = true;
                    bottomCard.IsFacedown = false;
                    OnMoveOneStep?.Invoke(CardMoveStep.FaceupPile);
                }
            }

            OnMoveOneStep?.Invoke(CardMoveStep.PileToPile);
        }

        private void OnClick(CardInfo card)
        {
            var isEnable = IsEnableMove?.Invoke();
            if (isEnable != null && !isEnable.Value)
                return;

            switch (card.CardType)
            {
                case CardType.Stock:
                    break;

                case CardType.Waste:
                {
                    // �R�D����߂������J�[�h -> �g�D�̈ړ�����
                    if (Utility.TryMoveToFoundation(_cards, card, out Suit suit))
                    {
                        WasteToFoundation(card, suit);
                    }
                    // �R�D����߂������J�[�h -> ��D�̈ړ�����
                    else if (Utility.TryMoveToPile(_cards, card, out int pileIndex))
                    {
                        WasteToPile(card, pileIndex);
                    }
                    break;
                }

                case CardType.Pile:
                {
                    var connectedCards = Utility.GetConnectedCards(_cards, card);

                    // ��D -> �g�D�̈ړ�����
                    if (connectedCards.Count == 0 && Utility.TryMoveToFoundation(_cards, card, out Suit suit))
                    {
                        PileToFoundation(card, suit);
                    }
                    // ��D -> ��D�̈ړ�����
                    else if (Utility.TryMoveToPile(_cards, card, out int pileIndex))
                    {
                        PileToPile(card, pileIndex, connectedCards);
                    }
                    break;
                }

                case CardType.Foundation:
                {
                    // �g�D -> ��D�̈ړ�����
                    if (Utility.TryMoveToPile(_cards, card, out int pileIndex))
                    {
                        // ��D�Ɉړ�
                        card.Order = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex).Count();
                        card.CardType = CardType.Pile;
                        card.PileIndex = pileIndex;

                        // �ړ���̏�D�̈ʒu���X�V
                        var pileCards = _cards.Where(c => c.CardType == CardType.Pile && c.PileIndex == pileIndex);
                        foreach (var c in pileCards)
                        {
                            OnMoveToPile?.Invoke(c, c.PileIndex, c.Order);
                        }

                        OnMoveOneStep?.Invoke(CardMoveStep.FoundationToPile);
                    }
                    break;
                }

                default:
                    break;
            }
        }

        private void OnBeginDrag(CardInfo card, Vector2 pos)
        {
            var isEnable = IsEnableMove?.Invoke();
            if (isEnable != null && isEnable.Value)
                card.IsDrag = true;
        }

        private void OnEndDrag(CardInfo card, Vector2 pos)
        {
            if (!card.IsDrag)
                return;

            var isEnable = IsEnableMove?.Invoke();
            if (isEnable != null && !isEnable.Value)
                return;

            switch (card.CardType)
            {
                case CardType.Stock:
                    break;

                case CardType.Waste:
                {
                    var move = false;

                    // �R�D����߂������J�[�h -> �g�D�̈ړ�����
                    for (var i = 0; i < _dropBounds.Foundations.Length; ++i)
                    {
                        if (_dropBounds.Foundations[i].Intersects(card.GetComponent<BoxCollider2D>().bounds)
                            && Utility.CanMoveToFoundation(_cards, card, (Suit)i))
                        {
                            WasteToFoundation(card, (Suit)i);
                            move = true;
                            break;
                        }
                    }

                    // �R�D����߂������J�[�h -> ��D�̈ړ�����
                    for (var i = 0; i < _dropBounds.Piles.Length; ++i)
                    {
                        if (_dropBounds.Piles[i].Intersects(card.GetComponent<BoxCollider2D>().bounds)
                            && Utility.CanMoveToPile(_cards, card, i))
                        {
                            WasteToPile(card, i);
                            move = true;
                            break;
                        }
                    }

                    if (!move)
                        OnMoveToPrev(card);
                    break;
                }

                case CardType.Pile:
                {
                    var move = false;
                    var connectedCards = Utility.GetConnectedCards(_cards, card);
                    var srcPileIndex = card.PileIndex;

                    // ��D -> �g�D�̈ړ�����
                    if (connectedCards.Count == 0)
                    {
                        for (var i = 0; i < _dropBounds.Foundations.Length; ++i)
                        {
                            if (_dropBounds.Foundations[i].Intersects(card.GetComponent<BoxCollider2D>().bounds)
                                && Utility.CanMoveToFoundation(_cards, card, (Suit)i))
                            {
                                PileToFoundation(card, (Suit)i);
                                move = true;
                                break;
                            }
                        }
                    }

                    // ��D -> ��D�̈ړ�����
                    for (var i = 0; i < _dropBounds.Piles.Length; ++i)
                    {
                        if (_dropBounds.Piles[i].Intersects(card.GetComponent<BoxCollider2D>().bounds)
                            && Utility.CanMoveToPile(_cards, card, i))
                        {
                            PileToPile(card, i, connectedCards);
                            move = true;
                            break;
                        }
                    }

                    if (!move)
                        OnMoveToPrev(card);
                    break;
                }

                case CardType.Foundation:
                {
                    var move = false;

                    // �g�D -> ��D�̈ړ�����
                    for (var i = 0; i < _dropBounds.Piles.Length; ++i)
                    {
                        if (_dropBounds.Piles[i].Intersects(card.GetComponent<BoxCollider2D>().bounds)
                            && Utility.CanMoveToPile(_cards, card, i))
                        {
                            FoundationToPile(card, i);
                            move = true;
                            break;
                        }
                    }

                    if (!move)
                        OnMoveToPrev(card);
                    break;
                }

                default:
                    break;
            }

            card.IsDrag = false;
        }
    }
}
