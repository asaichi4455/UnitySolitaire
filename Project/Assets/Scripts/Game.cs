using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Solitaire
{
    public class Game : MonoBehaviour
    {
        private enum State
        {
            Load,
            SelectDifficulty,
            Prepare,
            Play,
            GameClear
        }

        public static readonly int MaxWastes = 3;
        public static readonly int NumPiles = 7;
        private static readonly Dictionary<CardMoveStep, int> Score = new Dictionary<CardMoveStep, int>()
        {
            { CardMoveStep.StockToWaste,       0 },
            { CardMoveStep.WasteToStock,    -100 },
            { CardMoveStep.WasteToPile,        5 },
            { CardMoveStep.WasteToFoundation, 10 },
            { CardMoveStep.PileToPile,         0 },
            { CardMoveStep.PileToFoundation,  15 },
            { CardMoveStep.FoundationToPile, -15 },
            { CardMoveStep.FaceupPile,         5 },
        };

        private State _state = State.Load;
        private List<CardInfo> _cards = new List<CardInfo>();
        private Logic _logic = new Logic();
        private Movement _movement = new Movement();
        private Audio _audio = new Audio();
        private AsyncOperationHandle<IList<Sprite>> _cardSpritesHandle;
        private AsyncOperationHandle<GameObject> _cardPrefabHandle;

        [SerializeField] private SelectDifficulty _selectDifficulty;
        [SerializeField] private MouseEventHandler _newGameButton;
        [SerializeField] private Transform _stock;
        [SerializeField] private Transform _waste;
        [SerializeField] private Transform _piles;
        [SerializeField] private Foundations _foundations;
        [SerializeField] private Information _information;

        private async void Start()
        {
            _audio.Init();
            await Task.WhenAll(
                LoadResourcesAsync(),
                _audio.LoadResourcesAsync()
            );

            Init();

            _state = State.SelectDifficulty;
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Load:
                    break;

                case State.SelectDifficulty:
                    break;

                case State.Prepare:
                    PrepareStock();
                    DealCards();
                    _state = State.Play;
                    break;

                case State.Play:
                    var time = _information.Time;
                    time += Time.deltaTime;
                    _information.Time = time;
                    break;

                case State.GameClear:
                    if (!_movement.IsAnimate)
                    {
                        _selectDifficulty.Type = SelectDifficulty.Dialog.GameClear;
                        _selectDifficulty.Active = true;
                        _state = State.SelectDifficulty;
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            _audio.ReleaseResources();
            ReleaseResources();
        }

        private async Task LoadResourcesAsync()
        {
            _cardSpritesHandle = Addressables.LoadAssetAsync<IList<Sprite>>("Textures/Card");
            _cardPrefabHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Card");
            await Task.WhenAll(
                _cardSpritesHandle.Task,
                _cardPrefabHandle.Task
            );
        }

        private void ReleaseResources()
        {
            Addressables.Release(_cardSpritesHandle);
            Addressables.Release(_cardPrefabHandle);
        }

        private void Init()
        {
            // カード生成
            for (Suit s = 0; s < Suit.Num; ++s)
            {
                for (Number n = 0; n < Number.Num; ++n)
                {
                    var card = Instantiate(_cardPrefabHandle.Result).GetComponent<CardInfo>();
                    card.Init(s, n, _cardSpritesHandle.Result.ToList());
                    _cards.Add(card);
                }
            }

            // 難易度選択
            _selectDifficulty.Init();
            _selectDifficulty.OnSelect += difficulty =>
            {
                _selectDifficulty.Active = false;
                _logic.NumTurnToWaste = Utility.GetNumTurnToWaste(difficulty);
                _information.Clear();
                _state = State.Prepare;
            };
            _selectDifficulty.OnCancel += () =>
            {
                _selectDifficulty.Active = false;
            };
            _selectDifficulty.Type = SelectDifficulty.Dialog.Init;
            _selectDifficulty.Active = true;

            // 新しいゲームボタン
            _newGameButton.OnClick += () =>
            {
                _selectDifficulty.Type = SelectDifficulty.Dialog.NewGame;
                _selectDifficulty.Active = true;
            };

            // 山札、組札
            var stock = _stock.GetComponent<Stock>();
            stock.Init(_cardSpritesHandle.Result.ToList());
            stock.OnClick += _logic.OnClickStock;
            _foundations.Init(_cardSpritesHandle.Result.ToList());

            // ロジック制御
            var bounds = CreateCardDropBounds();
            _logic.Init(_cards, bounds);
            _logic.OnMoveStockToWaste += _movement.MoveStockToWaste;
            _logic.OnMoveWasteToStock += _movement.MoveWasteToStock;
            _logic.OnMoveWaste += _movement.MoveWaste;
            _logic.OnMoveWasteFull += _movement.MoveWasteFull;
            _logic.OnMoveToPile += (card, pileIndex, order) => { var _ = _movement.MoveToPile(card, pileIndex, order); };
            _logic.OnMoveToFoundation += (card, order, isClear) => { var _ = _movement.MoveToFoundation(card, order, isClear); };
            _logic.OnMoveToPrev += card => { var _ = _movement.MoveToPrev(card); };
            _logic.OnMoveOneStep += step =>
            {
                if (step != CardMoveStep.FaceupPile)
                {
                    _information.Move += 1;
                    _audio.OnMoveOneStep(step);
                }
                _information.Score += Score[step];
            };
            _logic.OnGameClear += () =>
            {
                _logic.SetCardsGameClear();
                _state = State.GameClear;
            };
            _logic.IsEnableMove += () => !_movement.IsAnimate;

            // 移動制御
            _movement.Init(_cards, CreateCardCoordinates(), bounds.Piles);
        }

        private void PrepareStock()
        {
            // シャッフルして山札に配置
            for (var i = _cards.Count - 1; i > 0; --i)
            {
                var j = Random.Range(0, i + 1);
                var tmp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = tmp;

                _cards[i].CardType = CardType.Stock;
                _cards[i].PileIndex = 0;
                _cards[i].Order = i;
                _cards[i].SortingOrder = i;
                _cards[i].Clickable = false;
                _cards[i].IsFacedown = true;
            }
            _movement.PrepareStock();
        }

        private void DealCards()
        {
            _logic.DealCards(NumPiles);
            _audio.OnDealCards();
        }

        private CardDropBounds CreateCardDropBounds()
        {
            var bounds = new CardDropBounds();
            var pileDropArea = _piles.GetComponentsInChildren<BoxCollider2D>();
            var foundationDropArea = _foundations.GetComponentsInChildren<BoxCollider2D>();
            bounds.Piles = new Bounds[pileDropArea.Length];
            bounds.Foundations = new Bounds[foundationDropArea.Length];
            for (var i = 0; i < pileDropArea.Length; ++i)
            {
                bounds.Piles[i] = pileDropArea[i].bounds;
            }
            for (var i = 0; i < foundationDropArea.Length; ++i)
            {
                bounds.Foundations[i] = foundationDropArea[i].bounds;
            }
            return bounds;
        }

        private CardCoordinates CreateCardCoordinates()
        {
            var coordinates = new CardCoordinates();
            var pileDropArea = _piles.GetComponentsInChildren<BoxCollider2D>();
            var foundationDropArea = _foundations.GetComponentsInChildren<BoxCollider2D>();
            coordinates.Stock = _stock.position;
            coordinates.Waste = _waste.position;
            coordinates.Piles = new Vector2[pileDropArea.Length];
            coordinates.Foundations = new Vector2[foundationDropArea.Length];
            for (var i = 0; i < pileDropArea.Length; ++i)
            {
                coordinates.Piles[i] = pileDropArea[i].transform.position;
            }
            for (var i = 0; i < foundationDropArea.Length; ++i)
            {
                coordinates.Foundations[i] = foundationDropArea[i].transform.position;
            }
            return coordinates;
        }
    }
}
