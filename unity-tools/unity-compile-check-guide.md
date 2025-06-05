# Unity Compile Check Script 完全ガイド

## 📋 概要

`unity-compile-check.sh` は、Unityプロジェクトのコンパイルエラーを安全かつ効率的にチェックするためのBashスクリプトです。LLM Agentによる自動化にも対応しています。

## 🎯 主な特徴

### ✅ 安全性
- **Unity終了なし**: エディターを強制終了することはありません
- **非破壊的動作**: 実行中の作業を中断しません
- **自動フォールバック**: 危険な操作は安全なモードに自動切り替え

### 🤖 LLM Agent対応
- **構造化出力**: JSON/Simple形式での結果出力
- **明確な戻り値**: 0=成功, 1=エラー, 2=実行失敗
- **自動モード**: ユーザーへの問い合わせなし

### 🔧 多機能
- **Unity自動検出**: プロジェクト固有のUnityバージョンを自動選択
- **ログ解析**: Editor.logから詳細なエラー情報を抽出
- **コンパイルトリガー**: Unityでの再コンパイルを外部から実行

## 🚀 使用方法

### LLM Agent推奨コマンド

```bash
# 最も安全なエラーチェック
./unity-compile-check.sh --check <path>

# コンパイルトリガー後チェック
./unity-compile-check.sh --trigger <path>

# JSON形式で結果出力
./unity-compile-check.sh --json --check <path>

# シンプル出力（成功/失敗のみ）
./unity-compile-check.sh --simple --check <path>
```

### 従来のオプション

```bash
# 基本的なログチェック
./unity-compile-check.sh <path>

# 詳細ログ付き
./unity-compile-check.sh -v <path>

# コンパイルトリガー
./unity-compile-check.sh -t <path>

# バッチモード（自動的に安全モードに切り替わる）
./unity-compile-check.sh -b <path>
```

## 📤 出力フォーマット

### 標準出力
```
📁 Project: /path/to/project
⚙️  Editor log check only
🔍 Checking Unity Editor logs...
📋 Using Editor log: /path/to/Editor.log
✅ Compilation successful
✅ Compilation check passed
```

### JSON出力
```json
{
  "success": true,
  "compilation": {
    "status": "success",
    "errors": [],
    "error_count": 0,
    "project_path": "/path/to/project",
    "timestamp": "2025-06-05T14:30:00Z"
  }
}
```

### エラー時のJSON出力
```json
{
  "success": false,
  "compilation": {
    "status": "failed",
    "errors": [
      "Assets/Scripts/Test.cs(10,5): error CS1002: ; expected"
    ],
    "error_count": 1,
    "project_path": "/path/to/project",
    "timestamp": "2025-06-05T14:30:00Z"
  }
}
```

### シンプル出力
```
SUCCESS
```
または
```
FAILED (2 errors)
```

## 🔧 オプション詳細

### Agent向けオプション
| オプション | 説明 | 安全性 |
|------------|------|---------|
| `--check` | 安全なエラーチェック | 🟢 最高 |
| `--trigger` | コンパイルトリガー | 🟡 中 |
| `--json` | JSON形式出力 | 🟢 出力のみ |
| `--simple` | シンプル出力 | 🟢 出力のみ |

### 従来オプション
| オプション | 説明 | 安全性 |
|------------|------|---------|
| `-e, --editor-only` | エディターログのみ（デフォルト） | 🟢 最高 |
| `-v, --verbose` | 詳細ログ表示 | 🟢 高 |
| `-t, --trigger-compile` | コンパイルトリガー | 🟡 中 |
| `-c, --compile` | エディターでコンパイル実行 | 🟡 中 |
| `-b, --batch-mode` | バッチモード（自動的に安全モードに切り替え） | 🟢 高 |
| `-a, --auto` | 自動モード（デフォルト） | 🟢 高 |

## 🛡️ 安全性機能

### 削除された危険機能
- ❌ Unity強制終了オプション（`-q, --quit`）
- ❌ Unity killプロセス
- ❌ 自動Unity終了ロジック

### 安全な代替動作
- ✅ バッチモードは自動的にエディターオンリーモードに切り替え
- ✅ Unityロック時は警告表示のみ
- ✅ すべての危険な操作はログチェックにフォールバック

## 🔍 エラー検出機能

### 対応エラーパターン
- **C#コンパイルエラー**: `error CS[0-9]{4}:`
- **ファイル位置付きエラー**: `Assets/*.cs(行,列): error`
- **コンパイラ出力**: `CompilerOutput:`セクション
- **一般的なエラー**: `Compilation failed`, `Build failed`
- **スクリプトエラー**: `Script compilation failed`

### エラー情報の詳細化
- ファイルパスと行番号の表示
- エラーメッセージの重複除去
- 最新エラーの優先表示

## 🤖 LLM Agent使用例

### 1. 基本的なエラーチェック
```bash
#!/bin/bash
result=$(./unity-compile-check.sh --simple --check . 2>/dev/null)
if [[ "$result" == "SUCCESS" ]]; then
    echo "✅ コンパイル成功"
    exit 0
else
    echo "❌ コンパイルエラー: $result"
    exit 1
fi
```

### 2. JSON解析による詳細処理
```bash
#!/bin/bash
json_result=$(./unity-compile-check.sh --json --check . 2>/dev/null)
error_count=$(echo "$json_result" | jq -r '.compilation.error_count')
errors=$(echo "$json_result" | jq -r '.compilation.errors[]')

if [[ "$error_count" -eq 0 ]]; then
    echo "コンパイル成功"
else
    echo "エラー数: $error_count"
    echo "詳細:"
    echo "$errors"
fi
```

### 3. 戻り値による処理分岐
```bash
#!/bin/bash
./unity-compile-check.sh --check .
exit_code=$?

case $exit_code in
    0)
        echo "✅ コンパイル成功"
        ;;
    1)
        echo "❌ コンパイルエラーが検出されました"
        ./unity-compile-check.sh --json --check . | jq '.compilation.errors'
        ;;
    2)
        echo "🔥 スクリプト実行エラー"
        ;;
esac
```

## 🔧 技術仕様

### 動作環境
- **OS**: macOS, Linux
- **Shell**: Bash 4.0+
- **Unity**: 任意のバージョン
- **依存ツール**: grep, sed, jq（JSON処理時）

### 内部動作フロー
1. **プロジェクト検証**: Unity プロジェクトの妥当性確認
2. **Unity検出**: 適切なUnityバージョンの自動検出
3. **安全性確認**: UnityLockfile等の状況確認
4. **モード選択**: ユーザー設定に基づく動作決定
5. **ログ解析**: エラーパターンの検出・報告
6. **結果出力**: 指定フォーマットでの結果表示

### パフォーマンス
- **実行時間**: 通常1-3秒
- **メモリ使用量**: 最小限
- **ファイルアクセス**: 読み取り専用
- **ネットワーク**: 不使用

## 🐛 トラブルシューティング

### よくある問題

#### 1. Unity ログファイルが見つからない
```
❌ No Unity log files found
```
**解決方法**: 
- Unityを一度起動してプロジェクトを開く
- `~/Library/Logs/Unity/`ディレクトリの存在確認

#### 2. 権限エラー
```
permission denied: ./unity-compile-check.sh
```
**解決方法**:
```bash
chmod +x unity-compile-check.sh
```

#### 3. JSONパースエラー
```
parse error: Invalid numeric literal
```
**解決方法**:
- `jq`コマンドのインストール確認
- JSON出力の手動確認

### デバッグモード
```bash
# 詳細ログでデバッグ
./unity-compile-check.sh -v --check .

# エラー出力も含めて確認
./unity-compile-check.sh --check . 2>&1
```

## 📈 パフォーマンス最適化

### 高速化のコツ
1. **エディターオンリーモード使用**: `-e` または `--check`
2. **JSON出力時はstderr抑制**: `2>/dev/null`
3. **シンプル出力使用**: 判定のみ必要な場合

### CI/CD環境での使用
```yaml
# GitHub Actions例
- name: Unity Compile Check
  run: |
    chmod +x unity-tools/unity-compile-check.sh
    ./unity-tools/unity-compile-check.sh --json --check . > compile-result.json
    if [ $? -ne 0 ]; then
      echo "コンパイルエラーが検出されました"
      cat compile-result.json
      exit 1
    fi
```

## 🔄 更新とメンテナンス

### バージョン確認
スクリプト内のコメントヘッダーでバージョン情報を確認

### アップデート方法
1. 新しいスクリプトをダウンロード
2. 実行権限を付与: `chmod +x unity-compile-check.sh`
3. 既存設定の移行（必要に応じて）

### カスタマイズ
- エラーパターンの追加: `grep -E`パターンを編集
- 出力フォーマットの変更: `output_json_result`関数を編集
- Unity検出ロジックの調整: `find_unity`関数を編集