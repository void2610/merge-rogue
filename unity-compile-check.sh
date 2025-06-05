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

# Unity パスは後で設定

###############################################################################
# 引数パース
###############################################################################

# デフォルト値
VERBOSE=false
QUIT_UNITY=false

# ヘルプ表示
show_help() {
    cat << EOF
Usage: $0 [options] <UnityProjectPath>

Options:
  -v, --verbose         詳細ログを表示
  -q, --quit            Unity エディターを強制終了してから実行
  -h, --help           このヘルプを表示

Examples:
  $0 /path/to/project                    # コンパイルチェック
  $0 -v /path/to/project                 # 詳細ログ付きコンパイルチェック
  $0 -q /path/to/project                 # Unity終了後にコンパイルチェック
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

# Unity パスの検出
UNITY=$(find_unity)
if [[ -z "$UNITY" ]]; then
    echo "❌ Unity not found. Please install Unity or update the script paths."
    exit 1
fi

echo "🔍 Using Unity: $UNITY"

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
    if [[ "$QUIT_UNITY" != "true" ]]; then
        echo "   Use -q option to automatically quit Unity, or close the editor manually"
        echo "   Continuing anyway..."
    fi
fi

echo "📁 Project: $PROJECT"
echo "⚙️  Compile check only"

###############################################################################
# Unity 実行
###############################################################################
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
if [[ $RET -eq 2 ]] || grep -q "Scripts have compiler errors" "$LOGFILE" || grep -q "CompilerOutput:" "$LOGFILE"; then
    HAS_COMPILE_ERRORS=true
fi

if [[ "$HAS_COMPILE_ERRORS" == "true" ]]; then
    echo "❌ Compile errors detected"
    echo "\n📋 Compiler errors:"
    
    # より広範囲のエラーパターンをキャッチ
    {
        grep -E "error CS[0-9]{4}:" "$LOGFILE" 2>/dev/null || true
        grep -E "Assets/.*\.cs\([0-9]+,[0-9]+\): error" "$LOGFILE" 2>/dev/null || true
        grep -A5 -B1 "CompilerOutput:" "$LOGFILE" 2>/dev/null || true
    } | sort -u | sed 's/^/    /'
    
    echo "\n❌ Compilation failed"
    RET=1
else
    echo "✅ Compilation successful"
    
    # Unity の基本情報を表示
    if [[ "$VERBOSE" == "true" ]]; then
        echo "\n📋 Project compilation details:"
        grep -E "(Successfully changed project path|Player connection|Assembly-CSharp)" "$LOGFILE" 2>/dev/null | head -3 | sed 's/^/    /' || true
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

# ログファイルのクリーンアップ（verbose モードでは保持）
if [[ "$VERBOSE" != "true" ]]; then
    rm -f "$LOGFILE"
else
    echo "📋 Log file preserved: $LOGFILE"
fi

exit $RET
