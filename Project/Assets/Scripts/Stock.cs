using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire
{
    public class Stock : MonoBehaviour
    {
        private static readonly string PlaceholderSpriteName = "PlaceholderStock";

        public event Action OnClick;

        public void Init(List<Sprite> sprites)
        {
            GetComponent<SpriteRenderer>().sprite = sprites?.Find(
                sprite => sprite.name == PlaceholderSpriteName
            );
            GetComponent<MouseEventHandler>().OnClick += () => OnClick?.Invoke();
        }
    }
}
