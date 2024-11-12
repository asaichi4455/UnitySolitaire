using System;
using TMPro;
using UnityEngine;

namespace Solitaire
{
    public class SelectDifficulty : MonoBehaviour
    {
        public enum Dialog { Init, NewGame, GameClear }
        public event Action<Difficulty> OnSelect;
        public event Action OnCancel;

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private MouseEventHandler _easy;
        [SerializeField] private MouseEventHandler _hard;
        [SerializeField] private MouseEventHandler _cancel;

        public Dialog Type
        {
            set
            {
                switch (value)
                {
                    case Dialog.Init:
                        _title.text = "難易度を選択";
                        _cancel.gameObject.SetActive(false);
                        break;

                    case Dialog.NewGame:
                        _title.text = "新しいゲームを\r\n開始しますか？";
                        _cancel.gameObject.SetActive(true);
                        break;

                    case Dialog.GameClear:
                        _title.text = "もう一度プレイ\r\nしますか？";
                        _cancel.gameObject.SetActive(false);
                        break;

                    default:
                        break;
                }
            }
        }

        public bool Active
        {
            set { gameObject.SetActive(value); }
        }

        public void Init()
        {
            _easy.OnClick += () => OnSelect?.Invoke(Difficulty.Easy);
            _hard.OnClick += () => OnSelect?.Invoke(Difficulty.Hard);
            _cancel.OnClick += () => OnCancel?.Invoke();
        }
    }
}
