using UnityEngine;

namespace Solitaire
{
    /// <summary>
    /// カードの絵柄
    /// </summary>
    public enum Suit
    {
        Heart, Diamond, Club, Spade, Num
    }

    /// <summary>
    /// カードの数字
    /// </summary>
    public enum Number
    {
        A, _2, _3, _4, _5, _6, _7, _8, _9, _10, J, Q, K, Num
    }

    /// <summary>
    /// 札の種類
    /// </summary>
    public enum CardType
    {
        Stock,      // 山札
        Waste,      // 山札からめくった札
        Pile,       // 場札
        Foundation  // 組札
    }

    /// <summary>
    /// 難易度
    /// </summary>
    public enum Difficulty
    {
        Easy, Hard
    }

    /// <summary>
    /// 操作の種類
    /// </summary>
    public enum CardMoveStep
    {
        StockToWaste,       // カード移動（山札 -> 山札からめくった札）
        WasteToStock,       // カード移動（山札からめくった札 -> 山札）
        WasteToPile,        // カード移動（山札からめくった札 -> 場札）
        WasteToFoundation,  // カード移動（山札からめくった札 -> 組札）
        PileToPile,         // カード移動（場札 -> 場札）
        PileToFoundation,   // カード移動（場札 -> 組札）
        FoundationToPile,   // カード移動（組札 -> 場札）
        FaceupPile          // 場札をめくる
    }

    /// <summary>
    /// カードの座標
    /// </summary>
    public class CardCoordinates
    {
        public Vector2 Stock;
        public Vector2 Waste;
        public Vector2[] Piles;
        public Vector2[] Foundations;
    }

    /// <summary>
    /// カードドラッグ時のドロップ可能領域
    /// </summary>
    public class CardDropBounds
    {
        public Bounds[] Piles;
        public Bounds[] Foundations;
    }
}
