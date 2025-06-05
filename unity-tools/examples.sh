#!/usr/bin/env bash
# Unity Tools 使用例集
# 
# このスクリプトは Unity Compile Check Script の様々な使用例を示します。
# 各関数は独立して使用できます。

# ============================================================================
# 基本的な使用例
# ============================================================================

# 例1: 基本的なエラーチェック
basic_check() {
    local project_path="${1:-.}"
    
    echo "=== 基本的なコンパイルチェック ==="
    ./unity-compile-check.sh --check "$project_path"
    
    case $? in
        0) echo "✅ コンパイル成功" ;;
        1) echo "❌ コンパイルエラーあり" ;;
        2) echo "🔥 スクリプト実行エラー" ;;
    esac
}

# 例2: JSON形式での詳細取得
json_check() {
    local project_path="${1:-.}"
    
    echo "=== JSON形式での結果取得 ==="
    result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    
    # JSONの妥当性チェック
    if ! echo "$result" | jq . >/dev/null 2>&1; then
        echo "❌ JSON解析エラー"
        return 1
    fi
    
    # 結果の解析
    success=$(echo "$result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "✅ コンパイル成功"
        echo "$result" | jq -r '.compilation.timestamp' | xargs -I {} echo "時刻: {}"
    else
        error_count=$(echo "$result" | jq -r '.compilation.error_count // 0')
        echo "❌ エラー数: $error_count"
        
        # エラー詳細を表示
        echo "$result" | jq -r '.compilation.errors[]? // empty' | while read -r error; do
            echo "  - $error"
        done
    fi
}

# 例3: シンプルな成功/失敗判定
simple_check() {
    local project_path="${1:-.}"
    
    echo "=== シンプルチェック ==="
    result=$(./unity-compile-check.sh --simple --check "$project_path" 2>/dev/null)
    echo "結果: $result"
    
    if [[ "$result" == "SUCCESS" ]]; then
        echo "✅ 処理を続行します"
        return 0
    else
        echo "❌ エラーを修正してください"
        return 1
    fi
}

# ============================================================================
# 高度な使用例
# ============================================================================

# 例4: エラー解析と自動修正提案
analyze_errors() {
    local project_path="${1:-.}"
    
    echo "=== エラー解析と修正提案 ==="
    
    result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    success=$(echo "$result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "✅ エラーなし"
        return 0
    fi
    
    # エラーの分類と提案
    echo "$result" | jq -r '.compilation.errors[]? // empty' | while read -r error; do
        echo "エラー: $error"
        
        # よくあるエラーパターンに基づく修正提案
        case "$error" in
            *"CS1002"*"expected"*)
                echo "  💡 修正提案: セミコロン(;)の追加を確認してください"
                ;;
            *"CS0103"*"does not exist"*)
                echo "  💡 修正提案: 変数名のスペルチェックまたはusing文の追加を確認してください"
                ;;
            *"CS1061"*"does not contain a definition"*)
                echo "  💡 修正提案: メソッド名やプロパティ名を確認してください"
                ;;
            *"CS0029"*"Cannot implicitly convert"*)
                echo "  💡 修正提案: 型の変換が必要です（キャストまたは適切な型の使用）"
                ;;
            *)
                echo "  💡 修正提案: Unityドキュメントまたはエラーコードで検索してください"
                ;;
        esac
        echo ""
    done
}

# 例5: 継続的監視
monitor_project() {
    local project_path="${1:-.}"
    local interval="${2:-10}"
    local max_duration="${3:-300}"  # 5分間
    
    echo "=== プロジェクト監視開始 ==="
    echo "プロジェクト: $project_path"
    echo "チェック間隔: ${interval}秒"
    echo "最大監視時間: ${max_duration}秒"
    echo ""
    
    local start_time=$(date +%s)
    local error_count=0
    local success_count=0
    
    while true; do
        local current_time=$(date +%s)
        local elapsed=$((current_time - start_time))
        
        # 最大監視時間をチェック
        if [[ $elapsed -gt $max_duration ]]; then
            echo "⏰ 監視時間終了"
            break
        fi
        
        # タイムスタンプ付きでチェック実行
        local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
        result=$(./unity-compile-check.sh --simple --check "$project_path" 2>/dev/null)
        
        echo "[$timestamp] $result"
        
        # 統計を更新
        if [[ "$result" == "SUCCESS" ]]; then
            ((success_count++))
        else
            ((error_count++))
            # エラー詳細を表示
            ./unity-compile-check.sh --json --check "$project_path" 2>/dev/null | \
                jq -r '.compilation.errors[]? // empty' | \
                head -3 | \
                sed 's/^/  /'
        fi
        
        sleep "$interval"
    done
    
    # 統計表示
    echo ""
    echo "=== 監視統計 ==="
    echo "成功: $success_count 回"
    echo "エラー: $error_count 回"
    echo "合計チェック: $((success_count + error_count)) 回"
}

# 例6: 複数プロジェクト一括チェック
batch_check() {
    local projects=("$@")
    
    if [[ ${#projects[@]} -eq 0 ]]; then
        echo "使用法: batch_check <project1> <project2> ..."
        return 1
    fi
    
    echo "=== 複数プロジェクト一括チェック ==="
    
    local results=()
    local total_errors=0
    
    for project in "${projects[@]}"; do
        if [[ ! -d "$project" ]]; then
            echo "⚠️  スキップ: $project (ディレクトリが存在しません)"
            continue
        fi
        
        echo ""
        echo "📁 チェック中: $project"
        echo "----------------------------------------"
        
        result=$(./unity-compile-check.sh --json --check "$project" 2>/dev/null)
        success=$(echo "$result" | jq -r '.success')
        
        if [[ "$success" == "true" ]]; then
            echo "✅ $project: 成功"
            results+=("$project:SUCCESS")
        else
            error_count=$(echo "$result" | jq -r '.compilation.error_count // 0')
            echo "❌ $project: $error_count エラー"
            results+=("$project:FAILED($error_count)")
            total_errors=$((total_errors + error_count))
            
            # 最初の3つのエラーを表示
            echo "$result" | jq -r '.compilation.errors[]? // empty' | head -3 | sed 's/^/  /'
        fi
    done
    
    # サマリー表示
    echo ""
    echo "=== 一括チェック結果 ==="
    for result in "${results[@]}"; do
        echo "  $result"
    done
    echo "合計エラー数: $total_errors"
}

# 例7: Git統合（コミット前チェック）
pre_commit_check() {
    local project_path="${1:-.}"
    
    echo "=== Git pre-commit チェック ==="
    
    # 変更されたC#ファイルをチェック
    changed_cs_files=$(git diff --cached --name-only --diff-filter=ACM | grep '\.cs$' || true)
    
    if [[ -z "$changed_cs_files" ]]; then
        echo "ℹ️  C#ファイルの変更なし - スキップ"
        return 0
    fi
    
    echo "変更されたC#ファイル:"
    echo "$changed_cs_files" | sed 's/^/  /'
    echo ""
    
    # コンパイルチェック実行
    result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    success=$(echo "$result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "✅ コンパイルチェック通過 - コミット可能"
        return 0
    else
        echo "❌ コンパイルエラーあり - コミット中止"
        echo ""
        echo "修正が必要なエラー:"
        echo "$result" | jq -r '.compilation.errors[]? // empty' | sed 's/^/  /'
        return 1
    fi
}

# 例8: パフォーマンス測定
performance_test() {
    local project_path="${1:-.}"
    local iterations="${2:-5}"
    
    echo "=== パフォーマンス測定 ==="
    echo "プロジェクト: $project_path"
    echo "実行回数: $iterations"
    echo ""
    
    local times=()
    local total_time=0
    
    for ((i=1; i<=iterations; i++)); do
        echo -n "実行 $i/$iterations ... "
        
        start_time=$(date +%s.%N)
        ./unity-compile-check.sh --simple --check "$project_path" >/dev/null 2>&1
        end_time=$(date +%s.%N)
        
        duration=$(echo "$end_time - $start_time" | bc)
        times+=("$duration")
        total_time=$(echo "$total_time + $duration" | bc)
        
        printf "%.3f秒\n" "$duration"
    done
    
    # 統計計算
    average=$(echo "scale=3; $total_time / $iterations" | bc)
    
    echo ""
    echo "=== 統計 ==="
    echo "平均実行時間: ${average}秒"
    echo "合計時間: $(printf "%.3f" "$total_time")秒"
    
    # 最小/最大時間
    min_time=$(printf '%s\n' "${times[@]}" | sort -n | head -1)
    max_time=$(printf '%s\n' "${times[@]}" | sort -n | tail -1)
    
    echo "最短時間: $(printf "%.3f" "$min_time")秒"
    echo "最長時間: $(printf "%.3f" "$max_time")秒"
}

# ============================================================================
# CI/CD統合例
# ============================================================================

# 例9: GitHub Actions風のチェック
ci_check() {
    local project_path="${1:-.}"
    
    echo "=== CI/CD風チェック ==="
    
    # 環境情報を表示
    echo "Environment Info:"
    echo "  OS: $(uname -s)"
    echo "  Date: $(date)"
    echo "  Project: $project_path"
    echo ""
    
    # Step 1: プロジェクト妥当性チェック
    echo "::group::Project Validation"
    if [[ ! -f "$project_path/ProjectSettings/ProjectVersion.txt" ]]; then
        echo "::error::Invalid Unity project"
        exit 1
    fi
    
    unity_version=$(grep "m_EditorVersion:" "$project_path/ProjectSettings/ProjectVersion.txt" | cut -d' ' -f2)
    echo "Unity Version: $unity_version"
    echo "::endgroup::"
    
    # Step 2: コンパイルチェック
    echo "::group::Compilation Check"
    result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    success=$(echo "$result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "::notice::Compilation successful"
        echo "✅ All checks passed"
    else
        echo "::error::Compilation failed"
        error_count=$(echo "$result" | jq -r '.compilation.error_count')
        echo "Error count: $error_count"
        
        # エラーをGitHub Actions形式で出力
        echo "$result" | jq -r '.compilation.errors[]? // empty' | while read -r error; do
            echo "::error::$error"
        done
        
        exit 1
    fi
    echo "::endgroup::"
    
    echo "🎉 CI check completed successfully"
}

# ============================================================================
# メイン関数（使用例のデモ）
# ============================================================================

show_help() {
    cat << EOF
Unity Tools 使用例集

使用法: $0 <example_name> [arguments...]

利用可能な例:
  basic-check [project]          基本的なエラーチェック
  json-check [project]           JSON形式での詳細取得
  simple-check [project]         シンプルな成功/失敗判定
  analyze-errors [project]       エラー解析と修正提案
  monitor [project] [interval]   継続的監視 (デフォルト:10秒間隔)
  batch-check <proj1> <proj2>... 複数プロジェクト一括チェック
  pre-commit [project]           Git pre-commitチェック
  performance [project] [runs]   パフォーマンス測定
  ci-check [project]             CI/CD風チェック

例:
  $0 basic-check .
  $0 monitor . 30
  $0 batch-check ./project1 ./project2
  $0 performance . 10

注意: 
  - [project] のデフォルトは現在のディレクトリ(.)です
  - JSON出力にはjqコマンドが必要です
EOF
}

# メイン処理
main() {
    if [[ $# -eq 0 ]]; then
        show_help
        exit 0
    fi
    
    local command="$1"
    shift
    
    case "$command" in
        basic-check)
            basic_check "$@"
            ;;
        json-check)
            json_check "$@"
            ;;
        simple-check)
            simple_check "$@"
            ;;
        analyze-errors)
            analyze_errors "$@"
            ;;
        monitor)
            monitor_project "$@"
            ;;
        batch-check)
            batch_check "$@"
            ;;
        pre-commit)
            pre_commit_check "$@"
            ;;
        performance)
            performance_test "$@"
            ;;
        ci-check)
            ci_check "$@"
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            echo "❌ 不明なコマンド: $command"
            echo ""
            show_help
            exit 1
            ;;
    esac
}

# スクリプトが直接実行された場合のみメイン関数を呼び出し
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi