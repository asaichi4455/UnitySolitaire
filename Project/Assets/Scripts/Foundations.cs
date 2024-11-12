using System.Collections.Generic;
using UnityEngine;

namespace Solitaire
{
    public class Foundations : MonoBehaviour
    {
        private static readonly Dictionary<Suit, string> SpriteNames = new Dictionary<Suit, string>()
        {
            { Suit.Heart,   "PlaceholderHeart"   },
            { Suit.Diamond, "PlaceholderDiamond" },
            { Suit.Club,    "PlaceholderClub"    },
            { Suit.Spade,   "PlaceholderSpade"   },
        };

        [SerializeField] private SpriteRenderer _heart;
        [SerializeField] private SpriteRenderer _diamond;
        [SerializeField] private SpriteRenderer _club;
        [SerializeField] private SpriteRenderer _spade;

        public void Init(List<Sprite> sprites)
        {
            _heart.sprite = sprites.Find(
                sprite => sprite.name == SpriteNames[Suit.Heart]
            );
            _diamond.sprite = sprites.Find(
                sprite => sprite.name == SpriteNames[Suit.Diamond]
            );
            _club.sprite = sprites.Find(
                sprite => sprite.name == SpriteNames[Suit.Club]
            );
            _spade.sprite = sprites.Find(
                sprite => sprite.name == SpriteNames[Suit.Spade]
            );
        }
    }
}
