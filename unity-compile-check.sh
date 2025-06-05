#!/usr/bin/env bash
# unity-compile-check.sh
# Unity コンパイルエラーチェックスクリプト
# 使い方: ./unity-compile-check.sh [options] <UnityProjectPath>

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
    # macOS のデフォルト Editor.log パス
    local editor_log="$HOME/Library/Logs/Unity/Editor.log"
    
    if [[ -f "$editor_log" ]]; then
        echo "$editor_log"
        return 0
    fi
    
    # 代替パスも探す
    local alt_paths=(
        "$HOME/Library/Logs/Unity Editor.log"
        "/var/folders/*/*/T/Unity/Editor.log"
    )
    
    for pattern in "${alt_paths[@]}"; do
        for log in $pattern; do
            if [[ -f "$log" ]]; then
                echo "$log"
                return 0
            fi
        done
    done
    
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

# Unity エディターで強制コンパイルを実行
force_unity_compile() {
    echo "🔄 Forcing Unity Editor to recompile..."
    
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

# Unity パスは後で設定

###############################################################################
# 引数パース
###############################################################################

# デフォルト値
VERBOSE=false
QUIT_UNITY=false
EDITOR_ONLY=false
FORCE_COMPILE=false

# ヘルプ表示
show_help() {
    cat << EOF
Usage: $0 [options] <UnityProjectPath>

Options:
  -v, --verbose         詳細ログを表示
  -q, --quit            Unity エディターを強制終了してから実行
  -e, --editor-only     エディターログのみをチェック（Unity実行せず）
  -f, --force-compile   Unity エディターで強制リコンパイルを実行
  -h, --help           このヘルプを表示

Examples:
  $0 /path/to/project                    # コンパイルチェック
  $0 -v /path/to/project                 # 詳細ログ付きコンパイルチェック
  $0 -q /path/to/project                 # Unity終了後にコンパイルチェック
  $0 -e /path/to/project                 # エディターログのみをチェック
  $0 -ef /path/to/project                # 強制コンパイル後にエディターログをチェック
EOF
}

# 引数解析
while [[ $# -gt 0 ]]; do
    case $1 in
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -q|--quit)
            QUIT_UNITY=true
            shift
            ;;
        -e|--editor-only)
            EDITOR_ONLY=true
            shift
            ;;
        -f|--force-compile)
            FORCE_COMPILE=true
            shift
            ;;
        -ef|-fe)
            EDITOR_ONLY=true
            FORCE_COMPILE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        -*)
            echo "❌ Unknown option: $1"
            show_help
            exit 1
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
    echo "❌ Unity project path is required"
    show_help
    exit 1
fi

if [[ ! -d "$PROJECT" ]]; then
    echo "❌ Project directory does not exist: $PROJECT"
    exit 1
fi

if [[ ! -f "$PROJECT/ProjectSettings/ProjectVersion.txt" ]]; then
    echo "❌ Not a valid Unity project: $PROJECT"
    exit 1
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

# Unity エディターを終了（オプション）
if [[ "$QUIT_UNITY" == "true" ]]; then
    echo "🔄 Quitting Unity Editor..."
    osascript -e 'quit app "Unity"' 2>/dev/null || true
    sleep 2
fi

# Unity プロジェクトがロックされているかチェック
if [[ -f "$PROJECT/Temp/UnityLockfile" ]]; then
    echo "⚠️  Unity project appears to be open in editor"
    if [[ "$QUIT_UNITY" != "true" ]] && [[ "$EDITOR_ONLY" != "true" ]]; then
        echo "   Use -q option to automatically quit Unity, or -e option to check editor logs only"
        echo "   Continuing anyway..."
    fi
fi

echo "📁 Project: $PROJECT"
if [[ "$EDITOR_ONLY" == "true" ]]; then
    echo "⚙️  Editor log check only"
else
    echo "⚙️  Compile check only"
fi

# 強制コンパイル実行
if [[ "$FORCE_COMPILE" == "true" ]]; then
    force_unity_compile
    if [[ $? -ne 0 ]]; then
        echo "⚠️  Force compile failed, continuing anyway..."
    fi
fi

###############################################################################
# Unity 実行 / ログチェック
###############################################################################

if [[ "$EDITOR_ONLY" == "true" ]]; then
    # エディターログのみをチェック
    echo "🔍 Checking Unity Editor logs..."
    EDITOR_LOG=$(find_unity_editor_log)
    CONSOLE_LOG=$(find_unity_console_log)
    
    if [[ -n "$EDITOR_LOG" ]]; then
        LOGFILE="$EDITOR_LOG"
        echo "📋 Using Editor log: $LOGFILE"
        RET=0
    elif [[ -n "$CONSOLE_LOG" ]]; then
        LOGFILE="$CONSOLE_LOG"
        echo "📋 Using Console log: $LOGFILE"
        RET=0
    else
        echo "❌ No Unity log files found"
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

# エラーパターンの検出（エディターログとUnity実行ログの両方に対応）
if [[ $RET -eq 2 ]] || \
   grep -q "Scripts have compiler errors" "$LOGFILE" 2>/dev/null || \
   grep -q "CompilerOutput:" "$LOGFILE" 2>/dev/null || \
   grep -q "error CS[0-9]\{4\}:" "$LOGFILE" 2>/dev/null || \
   grep -q "Assets/.*\.cs([0-9]\+,[0-9]\+): error" "$LOGFILE" 2>/dev/null || \
   grep -q "Compilation failed" "$LOGFILE" 2>/dev/null; then
    HAS_COMPILE_ERRORS=true
fi

if [[ "$HAS_COMPILE_ERRORS" == "true" ]]; then
    echo "❌ Compile errors detected"
    echo "\n📋 Compiler errors:"
    
    # より広範囲のエラーパターンをキャッチ
    {
        # C# コンパイルエラー
        grep -E "error CS[0-9]{4}:" "$LOGFILE" 2>/dev/null || true
        # ファイル位置付きエラー
        grep -E "Assets/.*\.cs\([0-9]+,[0-9]+\): error" "$LOGFILE" 2>/dev/null || true
        # CompilerOutput セクション
        grep -A5 -B1 "CompilerOutput:" "$LOGFILE" 2>/dev/null || true
        # 一般的なコンパイルエラーメッセージ
        grep -E "(Compilation failed|Build failed)" "$LOGFILE" 2>/dev/null || true
        # Unity エディターログの場合の追加パターン
        if [[ "$EDITOR_ONLY" == "true" ]]; then
            grep -E "(Script compilation failed|Assembly compilation failed)" "$LOGFILE" 2>/dev/null || true
            # 最近のエラーのみを表示（最後の100行から）
            tail -100 "$LOGFILE" | grep -E "error CS[0-9]{4}:" 2>/dev/null || true
        fi
    } | sort -u | sed 's/^/    /'
    
    echo "\n❌ Compilation failed"
    RET=1
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

###############################################################################
# 最終メッセージ
###############################################################################
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

# ログファイルのクリーンアップ（verbose モードまたはエディターログの場合は保持）
if [[ "$VERBOSE" != "true" ]] && [[ "$EDITOR_ONLY" != "true" ]]; then
    rm -f "$LOGFILE"
else
    if [[ "$EDITOR_ONLY" != "true" ]]; then
        echo "📋 Log file preserved: $LOGFILE"
    fi
fi

exit $RET
