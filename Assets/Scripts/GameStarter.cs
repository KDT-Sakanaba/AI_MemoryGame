using System;
using UnityEngine;
using MemoryGame.GameLogic;

public sealed class RealClock : IClock
{
    public double Now => Time.realtimeSinceStartupAsDouble;
}

/// <summary>シーン起動：GameRules を作り、Board/Input/Hud を接続する。</summary>
public sealed class GameStarter : MonoBehaviour
{
    [SerializeField] private BoardView board;
    [SerializeField] private InputHandler input;
    [SerializeField] private HudView hud;

    private GameRules rules;

    private void Start()
    {
        rules = new GameRules(new Deck(Environment.TickCount), new RealClock());
        board.Build(rules);
        input.Bind(rules);
        hud.Bind(rules);
    }

    private void Update()
    {
        if (rules != null) rules.Tick(); // 不一致ペアの約1秒後の裏返しを処理
    }
}
