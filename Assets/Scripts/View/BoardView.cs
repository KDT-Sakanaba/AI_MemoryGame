using UnityEngine;
using MemoryGame.GameLogic;

/// <summary>6x8 グリッドに CardView を配置し、GameRules と対応付ける。</summary>
public sealed class BoardView : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float cellSize = 1.2f;

    private GameRules rules;

    public void Build(GameRules gameRules)
    {
        rules = gameRules;
        foreach (Transform child in transform) Destroy(child.gameObject);

        foreach (var card in rules.Cards)
        {
            int row = card.GridIndex / Deck.Columns;
            int col = card.GridIndex % Deck.Columns;
            var go = Instantiate(cardPrefab, transform);
            go.name = $"Card_{card.Suit}_{card.Number}_{card.GridIndex}";
            go.transform.localPosition = new Vector3(col * cellSize, 0f, row * cellSize);
            var view = go.GetComponent<CardView>();
            view.Init(card);
            view.GetComponent<Collider>().isTrigger = false;
        }
        rules.StateChanged += OnChanged;
    }

    private void OnChanged(CardModel card)
    {
        var view = FindView(card);
        if (view != null) view.OnModelStateChanged(card);
    }

    private CardView FindView(CardModel card)
    {
        foreach (Transform child in transform)
        {
            var v = child.GetComponent<CardView>();
            if (v != null && v.Model == card) return v;
        }
        return null;
    }
}
