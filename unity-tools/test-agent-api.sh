#!/usr/bin/env bash
# LLM Agent API テストスクリプト

echo "=== LLM Agent向けUnity Compile Check API テスト ==="
echo ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$(dirname "$SCRIPT_DIR")"

echo "1. 基本チェック（安全モード）:"
"$SCRIPT_DIR/unity-compile-check.sh" --check "$PROJECT_PATH" 2>/dev/null
echo "戻り値: $?"
echo ""

echo "2. JSON形式での結果取得:"
"$SCRIPT_DIR/unity-compile-check.sh" --json --check "$PROJECT_PATH" 2>/dev/null
echo "戻り値: $?"
echo ""

echo "3. シンプル形式での結果取得:"
result=$("$SCRIPT_DIR/unity-compile-check.sh" --simple --check "$PROJECT_PATH" 2>/dev/null)
echo "結果: $result"
echo "戻り値: $?"
echo ""

echo "4. コンパイルトリガー付きチェック:"
"$SCRIPT_DIR/unity-compile-check.sh" --simple --trigger "$PROJECT_PATH" 2>/dev/null
echo "戻り値: $?"
echo ""

echo "=== Agent API 使用方法 ==="
echo "- 最も安全: ./unity-compile-check.sh --check <path>"
echo "- JSON出力: ./unity-compile-check.sh --json --check <path>"
echo "- 簡潔出力: ./unity-compile-check.sh --simple --check <path>"
echo "- 戻り値: 0=成功, 1=コンパイルエラー, 2=実行エラー"
echo ""
echo "=== 安全性機能 ==="
echo "- Unity終了機能は削除済み（安全のため）"
echo "- デフォルトはエディターログのみ確認"
echo "- バッチモードも自動的に安全モードに切り替わる"