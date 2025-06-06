#!/usr/bin/env bash
# unity-compile-check.sh
# Unity コンパイルエラーチェックスクリプト
# 使い方: ./unity-compile-check.sh [options] <UnityProjectPath>
#
# LLM Agent API:
# このスクリプトはLLM Agentが簡単に使用できるように設計されています
#
# === Agent推奨コマンド ===
#   ./unity-compile-check.sh --check <path>          # 最も安全なエラーチェック
#   ./unity-compile-check.sh --trigger <path>        # コンパイルトリガー後チェック
#   ./unity-compile-check.sh --json --check <path>   # JSON形式で結果出力
#   ./unity-compile-check.sh --simple --check <path> # シンプル出力（SUCCESS/FAILED）
#
# === JSON出力フォーマット ===
#   成功時: {"success": true, "compilation": {"status": "success", "errors": [], "error_count": 0, ...}}
#   失敗時: {"success": false, "compilation": {"status": "failed", "errors": [...], "error_count": N, ...}}
#   エラー時: {"success": false, "error": "error_type", "message": "error description"}
#
# === 戻り値 ===
#   0: コンパイル成功
#   1: コンパイルエラーあり
#   2: スクリプト実行エラー
#
# === Agent使用上の注意 ===
#   - --check は最も安全（Unityエディターを終了しない）
#   - --json は機械的な処理に最適
#   - --simple は成功/失敗の判定のみ必要な場合に使用
#   - 自動モードがデフォルト（ユーザーへの問い合わせなし）
#   - Unity終了機能は安全性のため削除済み
#   - バッチモードは自動的にエディターオンリーモードに切り替わる

# Unity パスの自動検出
find_unity() {
    # プロジェクトのUnityバージョンを取得
    local project_version=""
    if [[ -f "$PROJECT/ProjectSettings/ProjectVersion.txt" ]]; then
        project_version=$(grep "m_EditorVersion:" "$PROJECT/ProjectSettings/ProjectVersion.txt" | cut -d' ' -f2)
    fi
    
    # プロジェクト固有のUnityを優先して探す
    if [[ -n "$project_version" ]]; then
        local specific_unity="/Applications/Unity/Hub/Editor/$project_version/Unity.app/Contents/MacOS/Unity"
        if [[ -f "$specific_unity" ]]; then
            echo "$specific_unity"
            return 0
        fi
    fi
    
    # フォールバック: 利用可能なUnityを探す
    local unity_paths=(
        "/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
        "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
        "/opt/Unity/Editor/Unity"
    )
    
    for pattern in "${unity_paths[@]}"; do
        for unity in $pattern; do
            if [[ -f "$unity" ]]; then
                echo "$unity"
                return 0
            fi
        done
    done
    
    return 1
}

# Unity Editor.logファイルのパスを取得
find_unity_editor_log() {
    # 候補となるUnityログパスを順番に確認
    local log_paths=(
        "$HOME/Library/Logs/Unity/Editor.log"           # 標準のmacOSパス
        "$HOME/Library/Logs/Unity Editor.log"           # 古い形式
        "$HOME/Library/Logs/Unity/Editor-prev.log"      # 前回のログ
        "/var/folders/*/*/T/Unity/Editor.log"           # 一時フォルダ
        "$PROJECT/Logs/Editor.log"                       # プロジェクト内ログ
        "$PROJECT/Library/LastBuild.log"                 # ビルドログ
    )
    
    # 既存のログファイルを順番に確認
    for log_path in "${log_paths[@]}"; do
        if [[ "$log_path" == *"*"* ]]; then
            # ワイルドカードを含むパスの場合
            for expanded_path in $log_path; do
                if [[ -f "$expanded_path" ]]; then
                    echo "$expanded_path"
                    return 0
                fi
            done
        else
            # 通常のパスの場合
            if [[ -f "$log_path" ]]; then
                echo "$log_path"
                return 0
            fi
        fi
    done
    
    # 最新のログファイルをタイムスタンプで検索
    local latest_log=$(find "$HOME/Library/Logs" -name "*Unity*" -name "*Editor*" -type f 2>/dev/null | head -1)
    if [[ -n "$latest_log" && -f "$latest_log" ]]; then
        echo "$latest_log"
        return 0
    fi
    
    return 1
}

# Unity Console.logファイルを探す
find_unity_console_log() {
    # Console.log はプロジェクト固有の場所にある場合が多い
    local console_logs=(
        "$PROJECT/Logs/AssetImportWorker0.log"
        "$PROJECT/Library/LastBuild.buildreport"
        "$HOME/Library/Logs/Unity/Editor-prev.log"
    )
    
    for log in "${console_logs[@]}"; do
        if [[ -f "$log" ]]; then
            echo "$log"
            return 0
        fi
    done
    
    return 1
}

# Unity エディターで強制コンパイルを実行（Unityを終了しない）
trigger_unity_compile() {
    echo "🔄 Triggering Unity Editor to recompile..."
    
    # Unity が実行中かチェック
    if ! pgrep -f "Unity" > /dev/null; then
        echo "❌ Unity Editor is not running"
        return 1
    fi
    
    # より単純なアプローチ: ファイルの変更時刻を更新してUnityに再インポートを促す
    echo "📝 Triggering file system watch for recompile..."
    
    # プロジェクト内のAssembly定義ファイルのタイムスタンプを更新
    local asmdef_files=(
        "$PROJECT/Assets/Scripts"
        "$PROJECT/Assets"
    )
    
    for dir in "${asmdef_files[@]}"; do
        if [[ -d "$dir" ]]; then
            find "$dir" -name "*.asmdef" -exec touch {} \; 2>/dev/null || true
            break
        fi
    done
    
    # より確実な方法：AppleScriptでキーボードショートカットを送信
    if osascript << 'EOF' 2>/dev/null; then
tell application "System Events"
    tell process "Unity"
        try
            -- Unity をアクティブにして Cmd+R を送信
            set frontmost to true
            delay 0.5
            keystroke "r" using {command down}
            delay 0.2
        on error
            -- エラーを無視
        end try
    end tell
end tell
EOF
        echo "✅ Recompile command sent to Unity"
    else
        echo "⚠️  AppleScript method failed, trying alternative..."
        
        # 代替: Unity プロセスにシグナルを送信
        pkill -USR1 Unity 2>/dev/null || true
        echo "📋 Fallback method attempted"
    fi
    
    # コンパイル完了を待つ
    echo "⏳ Waiting for compilation to complete..."
    sleep 4
    return 0
}

# Unity エディターで強制コンパイルを実行（従来の関数・後方互換性のため）
force_unity_compile() {
    trigger_unity_compile
}

# Unity終了機能は安全性のため削除されました
# Unityを手動で終了してからバッチモードを使用してください

# JSON出力用関数
output_json_result() {
    local success="$1"
    local has_errors="$2"
    local error_count="$3"
    local error_messages="$4"
    local project_path="$5"
    
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    
    if [[ "$success" == "true" ]]; then
        cat << EOF
{
  "success": true,
  "compilation": {
    "status": "success",
    "errors": [],
    "error_count": 0,
    "project_path": "$project_path",
    "timestamp": "$timestamp"
  }
}
EOF
    else
        # エラーメッセージをJSONエスケープ
        local escaped_errors=$(echo "$error_messages" | sed 's/\\/\\\\/g' | sed 's/"/\\"/g' | tr '\n' ' ' | sed 's/  */ /g')
        cat << EOF
{
  "success": false,
  "compilation": {
    "status": "failed",
    "errors": ["$escaped_errors"],
    "error_count": $error_count,
    "project_path": "$project_path",
    "timestamp": "$timestamp"
  }
}
EOF
    fi
}

# シンプル出力用関数
output_simple_result() {
    local success="$1"
    local error_count="$2"
    
    if [[ "$success" == "true" ]]; then
        echo "SUCCESS"
    else
        echo "FAILED ($error_count errors)"
    fi
}

# Unity パスは後で設定

###############################################################################
# 引数パース
###############################################################################

# デフォルト値
VERBOSE=false
EDITOR_ONLY=true  # デフォルトをエディターログチェックに変更
FORCE_COMPILE=false
AUTO_MODE=true    # LLM Agent向けに自動モードをデフォルトに
TRIGGER_COMPILE=false
JSON_OUTPUT=false
SIMPLE_MODE=false

# ヘルプ表示
show_help() {
    cat << EOF
Usage: $0 [options] <UnityProjectPath>

=== LLM Agent 推奨コマンド ===
  --check <path>              安全なコンパイルエラーチェック
  --trigger <path>            Unity側でコンパイルトリガー
  --json --check <path>       JSON形式で結果出力
  --simple --check <path>     シンプル出力（成功/失敗のみ）

=== 詳細オプション ===
  -v, --verbose               詳細ログを表示
  -e, --editor-only           エディターログのみをチェック[デフォルト]
  -c, --compile               Unity エディターでコンパイルを実行
  -t, --trigger-compile       Unity側でコンパイルをトリガー
  -b, --batch-mode            バッチモードでUnityを実行
  -a, --auto                  自動モード[デフォルト]
  --json                      JSON形式で出力
  --simple                    シンプル出力モード
  -h, --help                  このヘルプを表示

=== Agent向け使用例 ===
  $0 --check .                          # 最も安全なエラーチェック
  $0 --json --check .                   # JSON形式で結果を取得
  $0 --trigger .                        # コンパイルを実行して結果チェック
  $0 --simple --check .                 # 簡潔な結果のみ
  
=== 従来の使用例 ===
  $0 .                                  # エディターログのみをチェック
  $0 -v .                               # 詳細ログ付きチェック
  $0 -t .                               # コンパイルトリガー
  $0 -b .                               # バッチモードで実行
EOF
}

# 引数解析
while [[ $# -gt 0 ]]; do
    case $1 in
        --check)
            # Agent用シンプルAPI: 安全なチェック
            EDITOR_ONLY=true
            AUTO_MODE=true
            shift
            if [[ -n "$1" && "$1" != -* ]]; then
                PROJECT="$1"
                shift
            fi
            ;;
        --trigger)
            # Agent用シンプルAPI: コンパイルトリガー
            TRIGGER_COMPILE=true
            EDITOR_ONLY=true
            AUTO_MODE=true
            shift
            if [[ -n "$1" && "$1" != -* ]]; then
                PROJECT="$1"
                shift
            fi
            ;;
        --json)
            JSON_OUTPUT=true
            shift
            ;;
        --simple)
            SIMPLE_MODE=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -e|--editor-only)
            EDITOR_ONLY=true
            shift
            ;;
        -c|--compile)
            FORCE_COMPILE=true
            EDITOR_ONLY=true
            shift
            ;;
        -t|--trigger-compile)
            TRIGGER_COMPILE=true
            EDITOR_ONLY=true
            shift
            ;;
        -b|--batch-mode)
            EDITOR_ONLY=false
            shift
            ;;
        -a|--auto)
            AUTO_MODE=true
            shift
            ;;
        -f|--force-compile)
            # 後方互換性のため残す
            FORCE_COMPILE=true
            shift
            ;;
        -ef|-fe|-ct|-tc)
            EDITOR_ONLY=true
            FORCE_COMPILE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        -*)
            if [[ "$JSON_OUTPUT" == "true" ]]; then
                echo '{"success": false, "error": "unknown_option", "message": "Unknown option: '$1'"}'
            else
                echo "❌ Unknown option: $1"
            fi
            exit 2
            ;;
        *)
            if [[ -z "$PROJECT" ]]; then
                PROJECT="$1"
            fi
            shift
            ;;
    esac
done

# プロジェクトパスの検証
if [[ -z "$PROJECT" ]]; then
    if [[ "$JSON_OUTPUT" == "true" ]]; then
        echo '{"success": false, "error": "missing_project_path", "message": "Unity project path is required"}'
    elif [[ "$SIMPLE_MODE" == "true" ]]; then
        echo "ERROR: Missing project path"
    else
        echo "❌ Unity project path is required"
        show_help
    fi
    exit 2
fi

if [[ ! -d "$PROJECT" ]]; then
    if [[ "$JSON_OUTPUT" == "true" ]]; then
        echo '{"success": false, "error": "project_not_found", "message": "Project directory does not exist: '$PROJECT'"}'
    elif [[ "$SIMPLE_MODE" == "true" ]]; then
        echo "ERROR: Project not found"
    else
        echo "❌ Project directory does not exist: $PROJECT"
    fi
    exit 2
fi

if [[ ! -f "$PROJECT/ProjectSettings/ProjectVersion.txt" ]]; then
    if [[ "$JSON_OUTPUT" == "true" ]]; then
        echo '{"success": false, "error": "invalid_unity_project", "message": "Not a valid Unity project: '$PROJECT'"}'
    elif [[ "$SIMPLE_MODE" == "true" ]]; then
        echo "ERROR: Invalid Unity project"
    else
        echo "❌ Not a valid Unity project: $PROJECT"
    fi
    exit 2
fi

# Unity パスの検出（editor-onlyモード以外）
if [[ "$EDITOR_ONLY" != "true" ]]; then
    UNITY=$(find_unity)
    if [[ -z "$UNITY" ]]; then
        echo "❌ Unity not found. Please install Unity or update the script paths."
        exit 1
    fi
    echo "🔍 Using Unity: $UNITY"
fi

###############################################################################
# 実行前準備
###############################################################################

# Unity エディターを終了機能は安全性のため削除されました

# Unity プロジェクトがロックされているかチェック
if [[ -f "$PROJECT/Temp/UnityLockfile" ]]; then
    # JSONやシンプルモード以外で警告表示
    if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
        echo "⚠️  Unity project appears to be open in editor"
    fi
    
    # エディターオンリーモードの場合は警告のみ
    if [[ "$EDITOR_ONLY" == "true" ]]; then
        if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
            echo "📋 Running in editor-only mode - will check existing logs safely"
        fi
    # バッチモードで明示的に実行する場合
    elif [[ "$EDITOR_ONLY" == "false" ]]; then
        # Unity終了機能は削除されたため、エディターオンリーモードに切り替え
        if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
            echo "⚠️  Batch mode requires Unity to be closed manually. Switching to safe editor-only mode..."
        fi
        EDITOR_ONLY=true
    fi
fi

# プロジェクト情報表示（JSONまたはシンプルモードでは非表示）
if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
    echo "📁 Project: $PROJECT"
    if [[ "$EDITOR_ONLY" == "true" ]]; then
        echo "⚙️  Editor log check only"
    else
        echo "⚙️  Compile check only"
    fi
fi

# コンパイルトリガー実行
if [[ "$TRIGGER_COMPILE" == "true" ]]; then
    trigger_unity_compile
    if [[ $? -ne 0 ]]; then
        echo "⚠️  Trigger compile failed, continuing anyway..."
    fi
elif [[ "$FORCE_COMPILE" == "true" ]]; then
    # 従来の-f/-cオプション対応
    trigger_unity_compile
    if [[ $? -ne 0 ]]; then
        echo "⚠️  Force compile failed, continuing anyway..."
    fi
fi

###############################################################################
# Unity 実行 / ログチェック
###############################################################################

if [[ "$EDITOR_ONLY" == "true" ]]; then
    # エディターログのみをチェック
    if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
        echo "🔍 Checking Unity Editor logs..."
    fi
    
    EDITOR_LOG=$(find_unity_editor_log)
    CONSOLE_LOG=$(find_unity_console_log)
    
    if [[ -n "$EDITOR_LOG" ]]; then
        LOGFILE="$EDITOR_LOG"
        if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
            echo "📋 Using Editor log: $LOGFILE"
        fi
        RET=0
    elif [[ -n "$CONSOLE_LOG" ]]; then
        LOGFILE="$CONSOLE_LOG"
        if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
            echo "📋 Using Console log: $LOGFILE"
        fi
        RET=0
    else
        if [[ "$JSON_OUTPUT" == "true" ]]; then
            echo '{"success": false, "error": "no_log_files", "message": "No Unity log files found"}'
        elif [[ "$SIMPLE_MODE" == "true" ]]; then
            echo "ERROR: No log files"
        else
            echo "❌ No Unity log files found"
        fi
        exit 1
    fi
else
    # 通常のUnity実行
    LOGFILE="$(mktemp -t unity_cli_XXXX).log"
    
    if [[ "$VERBOSE" == "true" ]]; then
        echo "📋 Log file: $LOGFILE"
    fi
    
    echo "🔨 Running compile check..."
    "$UNITY" \
      -batchmode -nographics \
      -projectPath "$PROJECT" \
      -quit \
      -logFile "$LOGFILE" 2>/dev/null
    RET=$?
fi

###############################################################################
# 出力処理
###############################################################################

# ログファイルが存在するかチェック
if [[ ! -f "$LOGFILE" ]]; then
    echo "❌ Log file not found: $LOGFILE"
    exit 1
fi

# 詳細ログ表示（オプション）
if [[ "$VERBOSE" == "true" ]]; then
    echo "\n📋 Full Unity log:"
    echo "========================"
    cat "$LOGFILE"
    echo "========================\n"
fi

# コンパイルエラーチェック
HAS_COMPILE_ERRORS=false

# エラーパターンの検出（Unity Editor.logの実際の形式に対応）
if [[ $RET -eq 2 ]] || \
   grep -q "Scripts have compiler errors" "$LOGFILE" 2>/dev/null || \
   grep -q "CompilerOutput:" "$LOGFILE" 2>/dev/null || \
   grep -q "error CS[0-9]\{4\}:" "$LOGFILE" 2>/dev/null || \
   grep -q "Assets/.*\.cs([0-9]\+,[0-9]\+): error CS" "$LOGFILE" 2>/dev/null || \
   grep -q "Compilation failed" "$LOGFILE" 2>/dev/null || \
   grep -q "Build failed" "$LOGFILE" 2>/dev/null || \
   grep -q "Script compilation failed" "$LOGFILE" 2>/dev/null; then
    HAS_COMPILE_ERRORS=true
fi

# エラーメッセージを収集
ERROR_MESSAGES=""
ERROR_COUNT=0

if [[ "$HAS_COMPILE_ERRORS" == "true" ]]; then
    # エラーメッセージを収集
    ERROR_MESSAGES=$(
        {
            # Unity Editor.logの実際の形式（Assets/path.cs(line,col): error CS####: message）
            grep -E "Assets/.*\.cs\([0-9]+,[0-9]+\): error CS[0-9]+:" "$LOGFILE" 2>/dev/null || true
            # C# コンパイルエラー（一般形式）
            grep -E "error CS[0-9]{4}:" "$LOGFILE" 2>/dev/null || true
            # CompilerOutput セクション
            grep -A5 -B1 "CompilerOutput:" "$LOGFILE" 2>/dev/null || true
            # 一般的なコンパイルエラーメッセージ
            grep -E "(Compilation failed|Build failed|Script compilation failed)" "$LOGFILE" 2>/dev/null || true
            # Unity エディターログの場合の追加パターン
            if [[ "$EDITOR_ONLY" == "true" ]]; then
                # 最近のエラーのみを表示（最後の200行から）
                tail -200 "$LOGFILE" | grep -E "Assets/.*\.cs\([0-9]+,[0-9]+\): error CS[0-9]+:" 2>/dev/null || true
                # Assembly compilation エラー
                grep -E "Assembly compilation failed" "$LOGFILE" 2>/dev/null || true
            fi
        } | sort -u | head -50
    )
    
    ERROR_COUNT=$(echo "$ERROR_MESSAGES" | grep -c "error" 2>/dev/null || echo 0)
    
    # 出力モードに応じた表示
    if [[ "$JSON_OUTPUT" == "true" ]]; then
        output_json_result "false" "true" "$ERROR_COUNT" "$ERROR_MESSAGES" "$PROJECT"
    elif [[ "$SIMPLE_MODE" == "true" ]]; then
        output_simple_result "false" "$ERROR_COUNT"
    else
        echo "❌ Compile errors detected"
        echo "\n📋 Compiler errors:"
        echo "$ERROR_MESSAGES" | sed 's/^/    /'
        echo "\n❌ Compilation failed"
    fi
    RET=1
else
    # 出力モードに応じた表示
    if [[ "$JSON_OUTPUT" == "true" ]]; then
        output_json_result "true" "false" "0" "" "$PROJECT"
    elif [[ "$SIMPLE_MODE" == "true" ]]; then
        output_simple_result "true" "0"
    else
        echo "✅ Compilation successful"
        
        # Unity の基本情報を表示
        if [[ "$VERBOSE" == "true" ]]; then
            echo "\n📋 Project compilation details:"
            if [[ "$EDITOR_ONLY" == "true" ]]; then
                echo "    Using editor log analysis"
                tail -10 "$LOGFILE" | grep -E "(Successfully|Compiled|Assembly-CSharp)" 2>/dev/null | sed 's/^/    /' || true
            else
                grep -E "(Successfully changed project path|Player connection|Assembly-CSharp)" "$LOGFILE" 2>/dev/null | head -3 | sed 's/^/    /' || true
            fi
        fi
    fi
fi

###############################################################################
# 最終メッセージ（JSONおよびシンプルモード以外のみ）
###############################################################################
if [[ "$JSON_OUTPUT" != "true" ]] && [[ "$SIMPLE_MODE" != "true" ]]; then
    echo ""
    if [[ $RET -eq 0 ]]; then
        echo "✅ Compilation check passed"
    else
        if [[ "$HAS_COMPILE_ERRORS" == "true" ]]; then
            echo "❌ Compilation failed"
        else
            echo "❌ Unity execution failed"
        fi
    fi
fi

# ログファイルのクリーンアップ（verbose モードまたはエディターログの場合は保持）
if [[ "$VERBOSE" != "true" ]] && [[ "$EDITOR_ONLY" != "true" ]]; then
    rm -f "$LOGFILE"
else
    if [[ "$EDITOR_ONLY" != "true" ]]; then
        echo "📋 Log file preserved: $LOGFILE"
    fi
fi

exit $RET
