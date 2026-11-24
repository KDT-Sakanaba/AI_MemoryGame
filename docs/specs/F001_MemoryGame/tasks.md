# F001: 神経衰弱（Memory Game） タスク

- 状態: 実装完了（Play モード確認待ち）
- Analyze: 合格（2026-11-24）
- 仕様: `spec.md` ／ 設計: `plan.md`

## タスク

- [x] T001: Unity プロジェクト骨格作成
  - 目的: MemoryGame を Unity 6000.6.4f1 プロジェクトとして成立させ、テスト・ビルドの土台を作る
  - 作業: `Unity.exe -batchmode -quit -createProject` で骨格生成（ProjectSettings/ProjectVersion.txt, Assets/）。Packages/manifest.json に Test Framework（com.unity.test-framework）と Input System（com.unity.inputsystem）を確認・追加。インプットアクティブ設定を確認（Physics Raycast 入力方式と整合させる）
  - 関連ファイル: ProjectSettings/ProjectVersion.txt, Packages/manifest.json, Assets/Scenes/
  - 依存: なし
  - 対応AC: AC-01（前提）
  - 完了条件: ProjectVersion.txt が 6000.6.4f1、manifest.json にテスト用パッケージが入り、バッチモードコンパイルが通る

- [x] T002: ゲームロジック（CardModel・Deck）
  - 目的: 48枚（4種×12番＝24ペア）の生成・シャッフル・グリッド（6×8）割り当て
  - 作業: `Assets/Scripts/GameLogic.asmdef`、`CardModel`（state: face-down/face-up/matched）、`Deck`（Fisher–Yates、6×8位置）
  - 関連ファイル: Assets/Scripts/GameLogic/CardModel.cs, Deck.cs, GameLogic.asmdef
  - 依存: T001
  - 対応AC: AC-01
  - 完了条件: EditMode テストで48枚・24ペア・全カード face-down・同一絵柄2枚ずつが検証できる

- [x] T003: 判定ルール（GameRules）
  - 目的: 2枚Flip判定、不一致約1秒後に裏へ戻す、手数・経過時間、クリア判定、リプレイ
  - 作業: `GameRules`（Flip 要求 API `RequestFlip(card)`、確定済み無視、同一カード連続要求を2枚目として扱わない、注入可能な時計で遅延判定、手数=2枚Flipで1回、全ペア matched でクリア、リプレイ=再シャッフル+手数/時間リセット）
  - 関連ファイル: Assets/Scripts/GameLogic/GameRules.cs
  - 依存: T002
  - 対応AC: AC-02, AC-03, AC-04, AC-05, AC-06, R8, R9
  - 完了条件: EditMode テストで判定・遅延（疑似時間）・手数・時間・クリア・リプレイが検証できる

- [x] T004: 表示・入力（View 層）
  - 目的: 3D カード表現、Flip 回転、6×8 グリッド配置、マウスクリック（Physics Raycast）、手数/時間/クリア表示、リプレイボタン
  - 作業: `CardView`（Cube メッシュ、Flip 180°回転、裏/表/確定の視覚区別）、`BoardView`（グリッド配置、CardModel 対応）、`InputHandler`（Raycast→GameRules）、`HudView`
  - 関連ファイル: Assets/Scripts/View/CardView.cs, BoardView.cs, InputHandler.cs, HudView.cs
  - 依存: T003
  - 対応AC: AC-01, AC-02, AC-03, AC-05（表示部分）
  - 完了条件: コンパイル通過、Play モードでカードがグリッド表示されクリックでめくれる

- [x] T005: シーン組み立て
  - 目的: シーンに BoardView/HudView/InputHandler を配置し、ロジックと View を接続
  - 作業: シーン作成（unity-cli のシーン API またはエディタ操作。YAML 手編集はしない）
  - 関連ファイル: Assets/Scenes/MemoryGame.unity
  - 依存: T004
  - 対応AC: AC-01〜AC-06（統合）
  - 完了条件: Play モードでゲームが最初から最後まで通して遊べる

- [x] T006: ビルド（コンパイル）確認
  - 目的: error CS なしを確認
  - 作業: unity-cli 経由（導入済みなら）または `Unity.exe -batchmode -quit -projectPath -logFile` でログから `error CS` 検索
  - 関連ファイル: ログファイル
  - 依存: T005
  - 対応AC: 全ACの前提
  - 完了条件: error CS ゼロ

- [x] T007: EditMode テスト実行
  - 目的: T002・T003 のテストを通過させる
  - 作業: unity-cli テスト API または `Unity.exe -batchmode -runTests -testPlatform EditMode -testResults`。失敗テストのみ抽出
  - 関連ファイル: Assets/Tests/EditMode/
  - 依存: T006
  - 対応AC: AC-01〜AC-06（ロジック部分）
  - 完了条件: 全テスト green

- [ ] T008: Play モード動作確認（ユーザー依頼）
  - 目的: 見た目・操作の受け入れ条件を確認
  - 作業: 手順と期待結果をユーザーに示す（48枚グリッド表示→クリックでめくる→不一致1秒後裏→確定ペア残る→クリア表示（手数/時間）→リプレイ）
  - 関連ファイル: なし
  - 依存: T007
  - 対応AC: AC-01〜AC-06（表示・操作部分）
  - 完了条件: ユーザー確認。確認されるまで「未確認」

## 再開メモ
- 最終更新日: 2026-11-24
- 完了: T001〜T007。プロジェクト骨格（6000.6.4f1）、GameLogic（CardModel/Deck/GameRules/IClock）、View（CardView/BoardView/InputHandler/HudView/GameStarter）、シーン Assets/Scenes/MemoryGame.unity、プレハブ Assets/Prefabs/Card.prefab
- 検証結果: コンパイル error CS なし（バッチモード exit=0）。EditMode テスト 62件全通過（62 passed / 0 failed）
- 設計との差異: plan.md の「同一絵柄2枚ずつ」は Clarify で「数字（番）のみでペア（48枚各1枚、24ペア）」へ修正済み。コード・テストは修正後の仕様に一致
- 未確認: T008（Play モードでの見た目・操作）はユーザー確認待ち
- 次に行う作業: ユーザーの Play モード確認（T008）
