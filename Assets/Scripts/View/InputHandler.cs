using UnityEngine;
using MemoryGame.GameLogic;

/// <summary>マウスクリック（Physics Raycast）でカードを GameRules へ渡す。
/// 将来「キャラが歩いて取り合う」方式へ差し替え可能な入口として RequestFlip 経由にする。</summary>
public sealed class InputHandler : MonoBehaviour
{
    private GameRules rules;

    public void Bind(GameRules gameRules) => rules = gameRules;

    private void Update()
    {
        if (rules == null || rules.Cleared) return;
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var view = hit.collider.GetComponent<CardView>();
                if (view != null && view.Model != null) rules.RequestFlip(view.Model);
            }
        }
    }
}
