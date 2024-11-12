using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire
{
    [RequireComponent(typeof(MouseEventHandler))]
    public class CardInfo : MonoBehaviour
    {
        private static readonly Dictionary<Suit, Dictionary<Number, string>> SpriteNames = new Dictionary<Suit, Dictionary<Number, string>>()
        {
            {
                Suit.Heart,
                new Dictionary<Number, string>()
                {
                    { Number.A,   "Heart_A"  },
                    { Number._2,  "Heart_2"  },
                    { Number._3,  "Heart_3"  },
                    { Number._4,  "Heart_4"  },
                    { Number._5,  "Heart_5"  },
                    { Number._6,  "Heart_6"  },
                    { Number._7,  "Heart_7"  },
                    { Number._8,  "Heart_8"  },
                    { Number._9,  "Heart_9"  },
                    { Number._10, "Heart_10" },
                    { Number.J,   "Heart_J"  },
                    { Number.Q,   "Heart_Q"  },
                    { Number.K,   "Heart_K"  },
                }
            },
            {
                Suit.Diamond,
                new Dictionary<Number, string>()
                {
                    { Number.A,   "Diamond_A"  },
                    { Number._2,  "Diamond_2"  },
                    { Number._3,  "Diamond_3"  },
                    { Number._4,  "Diamond_4"  },
                    { Number._5,  "Diamond_5"  },
                    { Number._6,  "Diamond_6"  },
                    { Number._7,  "Diamond_7"  },
                    { Number._8,  "Diamond_8"  },
                    { Number._9,  "Diamond_9"  },
                    { Number._10, "Diamond_10" },
                    { Number.J,   "Diamond_J"  },
                    { Number.Q,   "Diamond_Q"  },
                    { Number.K,   "Diamond_K"  },
                }
            },
            {
                Suit.Club,
                new Dictionary<Number, string>()
                {
                    { Number.A,   "Club_A"  },
                    { Number._2,  "Club_2"  },
                    { Number._3,  "Club_3"  },
                    { Number._4,  "Club_4"  },
                    { Number._5,  "Club_5"  },
                    { Number._6,  "Club_6"  },
                    { Number._7,  "Club_7"  },
                    { Number._8,  "Club_8"  },
                    { Number._9,  "Club_9"  },
                    { Number._10, "Club_10" },
                    { Number.J,   "Club_J"  },
                    { Number.Q,   "Club_Q"  },
                    { Number.K,   "Club_K"  },
                }
            },
            {
                Suit.Spade,
                new Dictionary<Number, string>()
                {
                    { Number.A,   "Spade_A"  },
                    { Number._2,  "Spade_2"  },
                    { Number._3,  "Spade_3"  },
                    { Number._4,  "Spade_4"  },
                    { Number._5,  "Spade_5"  },
                    { Number._6,  "Spade_6"  },
                    { Number._7,  "Spade_7"  },
                    { Number._8,  "Spade_8"  },
                    { Number._9,  "Spade_9"  },
                    { Number._10, "Spade_10" },
                    { Number.J,   "Spade_J"  },
                    { Number.Q,   "Spade_Q"  },
                    { Number.K,   "Spade_K"  },
                }
            },
        };
        private static readonly string BackSpriteName = "Facedown";

        public event Action<CardInfo> OnClick;
        public event Action<CardInfo, Vector2> OnBeginDrag;
        public event Action<CardInfo, Vector2> OnDrag;
        public event Action<CardInfo, Vector2> OnEndDrag;

        private Card _card = new Card();
        private List<Sprite> _sprites = null;
        private CardType _cardType = CardType.Stock;
        private int _pileIndex = 0;
        private int _order = 0;
        private Vector2 _prevPosition = Vector2.zero;
        private int _prevSortingOrder = 0;

        public Suit Suit
        {
            get { return _card.Suit; }
        }

        public Number Number
        {
            get { return _card.Number; }
        }

        public CardType CardType
        {
            get { return _cardType; }
            set { _cardType = value; }
        }

        public int PileIndex
        {
            get { return _pileIndex; }
            set { _pileIndex = value; }
        }

        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public Vector2 PrevPosition
        {
            get { return _prevPosition; }
            set { _prevPosition = value; }
        }

        public int PrevSortingOrder
        {
            get { return _prevSortingOrder; }
            set { _prevSortingOrder = value; }
        }

        public int SortingOrder
        {
            get { return GetComponent<SpriteRenderer>().sortingOrder; }
            set { GetComponent<SpriteRenderer>().sortingOrder = value; }
        }

        public bool Clickable
        {
            set { GetComponent<BoxCollider2D>().enabled = value; }
        }

        public bool IsFacedown
        {
            get
            {
                return GetComponent<SpriteRenderer>().sprite.name == BackSpriteName;
            }
            set
            {
                var spriteName = value ? BackSpriteName : SpriteNames[_card.Suit][_card.Number];
                GetComponent<SpriteRenderer>().sprite = _sprites?.Find(
                    sprite => sprite.name == spriteName
                );
            }
        }

        public Vector2 Size
        {
            get { return GetComponent<SpriteRenderer>().size; }
        }

        public void Init(Suit suit, Number number, List<Sprite> sprites)
        {
            _card.Suit = suit;
            _card.Number = number;
            _sprites = sprites;
            GetComponent<SpriteRenderer>().sprite = sprites?.Find(
                sprite => sprite.name == SpriteNames[suit][number]
            );

            var mouseEventHandler = GetComponent<MouseEventHandler>();
            mouseEventHandler.OnClick += () => OnClick?.Invoke(this);
            mouseEventHandler.OnBeginDrag += pos => OnBeginDrag?.Invoke(this, pos);
            mouseEventHandler.OnDrag += pos => OnDrag?.Invoke(this, pos);
            mouseEventHandler.OnEndDrag += pos => OnEndDrag?.Invoke(this, pos);
        }
    }
}
