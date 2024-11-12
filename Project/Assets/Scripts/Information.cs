using TMPro;
using UnityEngine;

namespace Solitaire
{
    public class Information : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _moveText;
        private float _time;
        private int _score;
        private int _move;

        public float Time
        {
            get { return _time; }
            set
            {
                _time = value;
                var t = (int)_time;
                var hour = t / 60 / 60;
                var minute = t / 60 % 60;
                var second = t % 60;
                _timeText.SetText($"{hour}:{minute:00}:{second:00}");
            }
        }

        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                if (_score < 0) _score = 0;
                _scoreText.SetText($"スコア　{_score}");
            }
        }

        public int Move
        {
            get { return _move; }
            set
            {
                _move = value;
                _moveText.SetText($"移動回数　{_move}");
            }
        }

        public void Clear()
        {
            Time = 0;
            Score = 0;
            Move = 0;
        }
    }
}
