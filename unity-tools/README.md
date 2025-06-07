# Unity Tools

このディレクトリには、Unity開発を効率化するためのシンプルなツール群が含まれています。

## unity-compile.sh

Unity エディターのコンパイル状態をチェックし、コンパイルをトリガーするシンプルなツールです。

### 機能

1. **リアルタイムコンパイルエラー取得** - Unityで現在発生しているコンパイルエラーを確認
2. **コンパイルトリガー** - Unityエディターでコンパイルを実行

### 使用方法

```bash
# 基本的な使い方
./unity-compile.sh [command] [project_path]

# 現在のコンパイルエラーをチェック
./unity-compile.sh check .

# Unityエディターでコンパイルを実行
./unity-compile.sh trigger .
```

### コマンド

- `check` - 現在のUnityコンパイルエラーを取得
- `trigger` - Unityエディターでコンパイルを実行

### 使用例

```bash
# プロジェクトのコンパイルエラーをチェック
./unity-compile.sh check /path/to/unity/project

# 現在のディレクトリでコンパイルエラーをチェック
./unity-compile.sh check .

# コンパイルを実行してからエラーチェック
./unity-compile.sh trigger .
./unity-compile.sh check .
```

### 出力例

**エラーがない場合:**
```
📋 Checking Unity log: /Users/user/Library/Logs/Unity/Editor.log
✅ No recent compilation errors detected
📝 Last compile status: CompileScripts: 1.603ms
```

**エラーがある場合:**
```
📋 Checking Unity log: /Users/user/Library/Logs/Unity/Editor.log
❌ Recent compilation errors found:
Assets/Scripts/Example.cs(11,9): error CS0103: The name 'NonExistentMethod' does not exist in the current context
```

### 技術仕様

- **対応OS**: macOS
- **依存関係**: Unity Editor（実行中である必要がある）
- **ログ解析**: Unity Editor.logの最新100行を解析
- **コンパイルトリガー**: AppleScriptを使用してCmd+Rショートカットを送信

### トラブルシューティング

**Unity Editor.log not found**
- Unityエディターが起動していることを確認してください

**Unityが実行中でない**
- `trigger`コマンドを使用する前にUnityエディターを起動してください

**コンパイルアクティビティが検出されない**
- Unityでファイルを変更してから少し待ってからチェックしてください
- `trigger`コマンドを使用してコンパイルを明示的に実行してください

### 設計思想

このツールは以下の原則で設計されています：

- **シンプル**: 2つの核心機能のみに集中
- **高速**: 不要な機能を排除して実行時間を最小化
- **正確**: 最新のログのみを解析して古いエラーを除外
- **実用的**: 日常の開発ワークフローに組み込みやすい

### 従来版からの改善

- **コードサイズ**: 657行 → 89行（86%削減）
- **機能の単純化**: 複雑なオプションとモードを削除
- **ログ解析の改善**: 最新のログのみをチェックして精度向上
- **保守性**: 理解しやすく修正しやすいコード構造