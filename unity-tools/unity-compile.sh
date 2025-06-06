#!/usr/bin/env bash
# Unity 簡単コンパイルツール
# 使い方: ./unity-compile.sh [check|trigger] [project_path]

# Unity Editor.logファイルを探す
find_unity_log() {
    local log_paths=(
        "$HOME/Library/Logs/Unity/Editor.log"
        "$HOME/Library/Logs/Unity Editor.log"
        "$HOME/Library/Logs/Unity/Editor-prev.log"
    )
    
    for log_path in "${log_paths[@]}"; do
        if [[ -f "$log_path" ]]; then
            echo "$log_path"
            return 0
        fi
    done
    
    return 1
}

# Unityの現在のコンパイルエラーを取得
get_compile_errors() {
    local log_file=$(find_unity_log)
    
    if [[ -z "$log_file" ]]; then
        echo "❌ Unity Editor.log not found"
        return 1
    fi
    
    echo "📋 Checking Unity log: $log_file"
    
    # 最新の100行のみをチェック（最近のログのみ）
    local recent_log=$(tail -100 "$log_file")
    
    # 最新のコンパイル完了メッセージをチェック
    local last_compile_success=$(echo "$recent_log" | grep -E "(CompileScripts|Assembly-CSharp.*compiled|Compilation finished|Scripts have compiler|Refresh completed)" | tail -1)
    
    # 最新のエラーメッセージをチェック
    local recent_errors=$(echo "$recent_log" | grep -E "Assets/.*\.cs\([0-9]+,[0-9]+\): error CS[0-9]+:")
    
    # コンパイル成功メッセージがエラーメッセージより後にある場合、エラーは解決済み
    if [[ -n "$last_compile_success" ]] && [[ -z "$recent_errors" ]]; then
        echo "✅ No recent compilation errors detected"
        echo "📝 Last compile status: $last_compile_success"
        return 0
    elif [[ -n "$recent_errors" ]]; then
        echo "❌ Recent compilation errors found:"
        echo "$recent_errors"
        return 1
    else
        echo "⚠️  No recent compilation activity detected"
        return 0
    fi
}

# Unityエディターでコンパイルをトリガー
trigger_compile() {
    echo "🔄 Triggering Unity Editor compilation..."
    
    # Unityが実行中かチェック
    if ! pgrep -f "Unity" > /dev/null; then
        echo "❌ Unity Editor is not running"
        return 1
    fi
    
    # AppleScriptでCmd+Rを送信してコンパイルをトリガー
    if osascript << 'EOF' 2>/dev/null; then
tell application "System Events"
    tell process "Unity"
        try
            set frontmost to true
            delay 0.5
            keystroke "r" using {command down}
        on error
            return false
        end try
    end tell
end tell
EOF
        echo "✅ Compile command sent to Unity"
        echo "⏳ Waiting for compilation..."
        sleep 3
        return 0
    else
        echo "❌ Failed to send compile command"
        return 1
    fi
}

# メイン処理
main() {
    local command="$1"
    local project_path="$2"
    
    # プロジェクトパスが指定されていればそこに移動
    if [[ -n "$project_path" ]]; then
        if [[ ! -d "$project_path" ]]; then
            echo "❌ Project directory not found: $project_path"
            exit 1
        fi
        cd "$project_path"
    fi
    
    case "$command" in
        check)
            get_compile_errors
            ;;
        trigger)
            trigger_compile
            ;;
        *)
            echo "Usage: $0 [check|trigger] [project_path]"
            echo ""
            echo "Commands:"
            echo "  check   - Get current Unity compilation errors"
            echo "  trigger - Trigger Unity editor compilation"
            echo ""
            echo "Examples:"
            echo "  $0 check ."
            echo "  $0 trigger ."
            exit 1
            ;;
    esac
}

main "$@"