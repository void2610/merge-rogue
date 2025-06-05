# Unity Tools

このフォルダには、Unityプロジェクトの開発を支援するツール群が含まれています。

## 📋 ツール一覧

### 1. Unity Compile Check Script
- **ファイル**: `unity-compile-check.sh`
- **目的**: Unityプロジェクトのコンパイルエラーを安全にチェック
- **特徴**: LLM Agent対応、JSON/Simple出力、非破壊的動作

### 2. Agent API Test Script
- **ファイル**: `test-agent-api.sh`
- **目的**: Unity Compile Check ScriptのAgent API機能をテスト
- **特徴**: 各種出力モードのデモンストレーション

## 🚀 クイックスタート

### 基本的な使用方法

```bash
# 最も安全なコンパイルチェック
./unity-tools/unity-compile-check.sh --check .

# JSON形式で結果を取得
./unity-tools/unity-compile-check.sh --json --check .

# シンプルな成功/失敗判定
./unity-tools/unity-compile-check.sh --simple --check .
```

### LLM Agent向け使用例

```bash
# 1. コンパイル状況の確認
result=$(./unity-tools/unity-compile-check.sh --simple --check . 2>/dev/null)
if [[ "$result" == "SUCCESS" ]]; then
    echo "コンパイル成功"
else
    echo "エラーあり: $result"
fi

# 2. JSON形式でのエラー詳細取得
./unity-tools/unity-compile-check.sh --json --check . 2>/dev/null | jq '.compilation.error_count'

# 3. 戻り値による処理分岐
./unity-tools/unity-compile-check.sh --check .
case $? in
    0) echo "成功" ;;
    1) echo "コンパイルエラー" ;;
    2) echo "実行エラー" ;;
esac
```

## 🛡️ 安全性機能

- **Unity終了なし**: スクリプトがUnityエディターを強制終了することはありません
- **非破壊的**: 実行中の作業を中断しません
- **フォールバック**: 危険な操作は自動的に安全なモードに切り替わります
- **予測可能**: すべての操作が一貫した動作を保証します

## 📖 詳細ドキュメント

各ツールの詳細な使用方法については、個別のドキュメントファイルを参照してください：

- [Unity Compile Check Script詳細](./unity-compile-check-guide.md)
- [LLM Agent API仕様](./llm-agent-api.md)

## 🔧 開発者向け情報

### 要件
- **OS**: macOS (Linux対応)
- **Unity**: 任意のバージョン
- **Shell**: Bash 4.0+
- **依存ツール**: grep, jq (JSON処理時)

### 設置方法
```bash
# スクリプトを実行可能にする
chmod +x unity-tools/*.sh

# PATHに追加（オプション）
export PATH="$PATH:$(pwd)/unity-tools"
```

## 📝 更新履歴

- **v2.0**: LLM Agent対応、JSON/Simple出力追加
- **v1.5**: 安全性向上（quit機能削除）
- **v1.0**: 基本的なコンパイルチェック機能