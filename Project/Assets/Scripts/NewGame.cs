using System;
using UnityEngine;

namespace Solitaire
{
    public class NewGame : MonoBehaviour
    {
        public event Action<Difficulty> OnSelect;
        public event Action OnCancel;

        [SerializeField] private MouseEventHandler _easy;
        [SerializeField] private MouseEventHandler _hard;
        [SerializeField] private MouseEventHandler _cancel;

        public bool Active
        {
            set
            {
                gameObject.SetActive(value);
            }
        }

        public void Init()
        {
            _easy.OnClick += () => OnSelect(Difficulty.Easy);
            _hard.OnClick += () => OnSelect(Difficulty.Hard);
            _cancel.OnClick += () => OnCancel();
        }
    }
}
