# F001: 神経衰弱（Memory Game） 設計

- 状態: 確定
- 仕様: `spec.md`

## 対象環境
- エンジン／フレームワーク: Unity（判定根拠: 新規プロジェクトとして `ProjectSettings/ProjectVersion.txt` と `Assets/` を作成）
- バージョン: 6000.6.4f1（確認済み: `C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe` 実在。unity-cli の Windows 検証は 6000.6.8f1 で PASS、6000.6.4f1 は近接バージョン）
- 言語: C#
- ビルド方法（コンパイル確認）: unity-cli 経由。導入済み・確認済み（2026-11-24: `C:\Users\koyo\.unity\tools\unity-cli.exe` v0.19.1、SHA256 検証 OK。GitHub API レート制限のため Quick Install スクリプトではなくリリースバイナリ直接取得）。ブリッジパッケージは T001 のプロジェクト作成後に MemoryGame へ導入（manifest.json 変更はユーザー承認済み）。フォールバック: `Unity.exe -batchmode -quit -projectPath <path> -logFile <log>` で `error CS` 確認
- テスト・動作確認方法: unity-cli のテスト API（導入後）または `Unity.exe -batchmode -runTests -testPlatform EditMode`。見た目・操作確認は Play モードでユーザー依頼

## 既存構造（調査結果）
- `C:\Users\koyo\Desktop\he\MemoryGame\`: 空（新規 Unity プロジェクトとして作成）
- `UnityCLI_test/Unity_CLI/`: unity-cli のソースリポジトリ（Rust、`UnityCliBridge` フォルダに Unity 側パッケージ）。MemoryGame には未導入
- 既存ゲームコードなし。テスト用 asmdef でロジックを分離（constitution 準拠）

## 設計

### アセンブリ構成
- `Assets/Scripts/GameLogic.asmdef`（テスト可能な純ロジック、UnityEngine 非依存寄り）
- `Assets/Tests/EditMode/GameLogic.Tests.asmdef`（Test Framework 参照）

### クラス／データ構造
- `CardModel`: 1枚分の状態（絵柄ID、face-down / face-up / matched）。めくる・確定する操作。判定は View へ通知しない（イベントまたは返り値）
- `Deck`: 48枚（4種×12番＝24ペア）生成、Fisher–Yates シャッフル、グリッド位置（6×8）への割り当て
- `GameRules`（または `MemoryGame`）: 2枚Flip判定、不一致は約1秒後に裏へ戻す（遅延はコルーチン/タイムスタンプで View 側）、手数カウント（2枚Flipで1回）、経過時間、全ペア揃った時点でクリア判定、リプレイ（Deck 再シャッフル・手数/時間リセット）
- `CardView`: 3D 表現（Cube メッシュまたは Sprite）。Flip は回転アニメーション（180°）。状態（裏/表/確定）を視覚区別（色/マテリアル違い）
- `BoardView`: 6×8 グリッド配置、CardModel と CardView の対応付け
- `InputHandler`: マウスクリック → Physics Raycast（3D）で CardView を解決 → GameRules へ。確定済みカードは無視
- `HudView`: 手数・経過時間表示、クリア表示、リプレイボタン

### 将来拡張への配慮（範囲外だが設計に影響する命名）
- 入力は「クリック」を直接 GameRules に渡さず、`RequestFlip(card)` のような操作API経由にしておく（将来キャラ移動による Flip 要求に差し替え可能）

## 変更範囲と依存関係
- 新規のみ: Unity プロジェクト一式（ProjectSettings, Packages/manifest.json, Assets/Scenes, Assets/Scripts, Assets/Tests）
- 依存: unity-cli + Unity ブリッジパッケージ（Packages/manifest.json への追加は導入スクリプト経由。ユーザー確認必須）
- Input System / Test Framework: manifest.json で確認・追加

## 検証方針
| 受け入れ条件 | 確認方法 |
|--------------|----------|
| AC-01 | EditMode テスト: Deck 生成で48枚・24ペア・全カード face-down。Play モードでグリッド表示をユーザー確認 |
| AC-02 | EditMode テスト: GameRules の Flip 状態遷移・マッチ判定。操作は Play モードユーザー確認 |
| AC-03 | EditMode テスト: 不一致ペアの「約1秒後に裏へ」を疑似時間（注入可能な時計）で検証 |
| AC-04 | EditMode テスト: matched カードへの Flip 要求は無視 |
| AC-05 | EditMode テスト: 全ペア matched でクリアフラグ。表示は Play モードユーザー確認 |
| AC-06 | EditMode テスト: 同一カードの連続 Flip 要求を2枚目として扱わない |
| 手数/時間（R8） | EditMode テスト: 手数加算、時計注入での経過時間 |
| リプレイ（R9） | EditMode テスト: リプレイ後、手数・時間リセット、全カード face-down、再シャッフル |

## リスク・代替案
- unity-cli 未導入: Quick Install（ネットワーク+PowerShell）はユーザー明示確認が必要。cargo ビルドは cargo 未インストールのため不可。フォールバックは Unity.exe -batchmode
- 6000.6.4f1 は unity-cli の Windows 検証マトリックス（6000.6.8f1）と完全一致ではない → 導入後 `unity-cli --version` とブリッジ接続で実確認
- シーン YAML の手編集は避ける（エンジン手順）。シーン作成はエディタ操作または unity-cli のシーン API

## 確認履歴
- 2026-11-24 Q: ビルド・テストに unity-cli を使うか → A: 使う（導入方法をこの plan に記録）。未導入のため Implement 前に導入確認
