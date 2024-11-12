using UnityEngine;

namespace Solitaire
{
    /// <summary>
    /// �J�[�h�̊G��
    /// </summary>
    public enum Suit
    {
        Heart, Diamond, Club, Spade, Num
    }

    /// <summary>
    /// �J�[�h�̐���
    /// </summary>
    public enum Number
    {
        A, _2, _3, _4, _5, _6, _7, _8, _9, _10, J, Q, K, Num
    }

    /// <summary>
    /// �D�̎��
    /// </summary>
    public enum CardType
    {
        Stock,      // �R�D
        Waste,      // �R�D����߂������D
        Pile,       // ��D
        Foundation  // �g�D
    }

    /// <summary>
    /// ��Փx
    /// </summary>
    public enum Difficulty
    {
        Easy, Hard
    }

    /// <summary>
    /// ����̎��
    /// </summary>
    public enum CardMoveStep
    {
        StockToWaste,       // �J�[�h�ړ��i�R�D -> �R�D����߂������D�j
        WasteToStock,       // �J�[�h�ړ��i�R�D����߂������D -> �R�D�j
        WasteToPile,        // �J�[�h�ړ��i�R�D����߂������D -> ��D�j
        WasteToFoundation,  // �J�[�h�ړ��i�R�D����߂������D -> �g�D�j
        PileToPile,         // �J�[�h�ړ��i��D -> ��D�j
        PileToFoundation,   // �J�[�h�ړ��i��D -> �g�D�j
        FoundationToPile,   // �J�[�h�ړ��i�g�D -> ��D�j
        FaceupPile          // ��D���߂���
    }

    /// <summary>
    /// �J�[�h�̍��W
    /// </summary>
    public class CardCoordinates
    {
        public Vector2 Stock;
        public Vector2 Waste;
        public Vector2[] Piles;
        public Vector2[] Foundations;
    }

    /// <summary>
    /// �J�[�h�h���b�O���̃h���b�v�\�̈�
    /// </summary>
    public class CardDropBounds
    {
        public Bounds[] Piles;
        public Bounds[] Foundations;
    }
}
