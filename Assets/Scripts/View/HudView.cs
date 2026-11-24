using UnityEngine;

/// <summary>手数・経過時間・クリア表示・リプレイボタン。</summary>
public sealed class HudView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text statsText;
    [SerializeField] private UnityEngine.UI.Text clearText;
    [SerializeField] private UnityEngine.UI.Button replayButton;

    private MemoryGame.GameLogic.GameRules rules;

    public void Bind(MemoryGame.GameLogic.GameRules gameRules)
    {
        rules = gameRules;
        if (replayButton != null) replayButton.onClick.AddListener(rules.Replay);
    }

    private void Update()
    {
        if (rules == null) return;
        if (statsText != null)
            statsText.text = $"Moves: {rules.Moves}   Time: {rules.Elapsed:0.0}s";
        if (clearText != null)
            clearText.text = rules.Cleared
                ? $"Cleared! Moves: {rules.Moves}  Time: {rules.Elapsed:0.0}s"
                : string.Empty;
    }
}
