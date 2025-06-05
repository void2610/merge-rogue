#!/usr/bin/env bash
# Unity Tools インストールスクリプト
# 
# このスクリプトはUnity Toolsの設置と設定を自動化します

set -e

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ログ関数
log_info() { echo -e "${BLUE}ℹ️  $1${NC}"; }
log_success() { echo -e "${GREEN}✅ $1${NC}"; }
log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
log_error() { echo -e "${RED}❌ $1${NC}"; }

# 設定
INSTALL_DIR="${HOME}/.local/bin"
TOOLS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

show_help() {
    cat << EOF
Unity Tools インストールスクリプト

使用法: $0 [options]

オプション:
  --install-dir DIR    インストール先ディレクトリ (デフォルト: ~/.local/bin)
  --global            システム全体にインストール (/usr/local/bin)
  --symlink           ファイルコピーではなくシンボリックリンクを作成
  --check             依存関係のチェックのみ実行
  --uninstall         Unity Toolsをアンインストール
  -h, --help          このヘルプを表示

例:
  $0                           # 標準インストール
  $0 --global                  # システム全体にインストール
  $0 --symlink                 # シンボリックリンクでインストール
  $0 --install-dir ~/bin       # カスタムディレクトリにインストール
EOF
}

# 依存関係チェック
check_dependencies() {
    log_info "依存関係をチェック中..."
    
    local missing_deps=()
    
    # 必須依存関係
    local required_deps=("bash" "grep" "sed" "jq")
    
    for dep in "${required_deps[@]}"; do
        if ! command -v "$dep" >/dev/null 2>&1; then
            missing_deps+=("$dep")
        fi
    done
    
    # macOS固有の依存関係
    if [[ "$OSTYPE" == "darwin"* ]]; then
        if ! command -v osascript >/dev/null 2>&1; then
            missing_deps+=("osascript (macOS標準)")
        fi
    fi
    
    if [[ ${#missing_deps[@]} -gt 0 ]]; then
        log_error "以下の依存関係が不足しています:"
        for dep in "${missing_deps[@]}"; do
            echo "  - $dep"
        done
        
        log_info "依存関係のインストール方法:"
        if command -v brew >/dev/null 2>&1; then
            echo "  brew install jq"
        elif command -v apt-get >/dev/null 2>&1; then
            echo "  sudo apt-get install jq"
        elif command -v yum >/dev/null 2>&1; then
            echo "  sudo yum install jq"
        fi
        
        return 1
    else
        log_success "すべての依存関係が満たされています"
        return 0
    fi
}

# Unity検出
check_unity() {
    log_info "Unity環境をチェック中..."
    
    local unity_found=false
    
    # macOS
    if [[ "$OSTYPE" == "darwin"* ]]; then
        if [[ -d "/Applications/Unity" ]]; then
            unity_found=true
            log_success "Unity Hub環境を検出しました"
        fi
    fi
    
    # Linux
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        if command -v unity-editor >/dev/null 2>&1; then
            unity_found=true
            log_success "Unity Editor環境を検出しました"
        fi
    fi
    
    if [[ "$unity_found" == false ]]; then
        log_warning "Unity環境が検出されませんでした"
        log_info "Unity Toolsは動作しますが、コンパイルトリガー機能は制限される可能性があります"
    fi
}

# インストール実行
install_tools() {
    local use_symlink="$1"
    
    log_info "Unity Toolsをインストール中..."
    
    # インストールディレクトリの作成
    if [[ ! -d "$INSTALL_DIR" ]]; then
        log_info "ディレクトリを作成: $INSTALL_DIR"
        mkdir -p "$INSTALL_DIR"
    fi
    
    # ツールファイルのリスト
    local tools=(
        "unity-compile-check.sh"
        "test-agent-api.sh"
        "examples.sh"
    )
    
    for tool in "${tools[@]}"; do
        local source_file="$TOOLS_DIR/$tool"
        local target_file="$INSTALL_DIR/$tool"
        
        if [[ ! -f "$source_file" ]]; then
            log_error "ソースファイルが見つかりません: $source_file"
            continue
        fi
        
        # 既存ファイルのバックアップ
        if [[ -f "$target_file" ]]; then
            log_warning "既存ファイルをバックアップ: $target_file.backup"
            cp "$target_file" "$target_file.backup"
        fi
        
        # インストール実行
        if [[ "$use_symlink" == "true" ]]; then
            log_info "シンボリックリンクを作成: $tool"
            ln -sf "$source_file" "$target_file"
        else
            log_info "ファイルをコピー: $tool"
            cp "$source_file" "$target_file"
            chmod +x "$target_file"
        fi
        
        log_success "インストール完了: $tool"
    done
    
    # ドキュメントの処理
    local docs_dir="$INSTALL_DIR/unity-tools-docs"
    if [[ ! -d "$docs_dir" ]]; then
        mkdir -p "$docs_dir"
    fi
    
    local docs=(
        "README.md"
        "unity-compile-check-guide.md"
        "llm-agent-api.md"
        "CHANGELOG.md"
    )
    
    for doc in "${docs[@]}"; do
        local source_file="$TOOLS_DIR/$doc"
        local target_file="$docs_dir/$doc"
        
        if [[ -f "$source_file" ]]; then
            cp "$source_file" "$target_file"
            log_success "ドキュメントをコピー: $doc"
        fi
    done
}

# PATH設定の確認と提案
check_path() {
    log_info "PATH設定をチェック中..."
    
    if [[ ":$PATH:" == *":$INSTALL_DIR:"* ]]; then
        log_success "PATH設定済み: $INSTALL_DIR"
    else
        log_warning "PATHが設定されていません: $INSTALL_DIR"
        log_info "以下のコマンドをシェル設定ファイルに追加してください:"
        echo ""
        echo "  export PATH=\"\$PATH:$INSTALL_DIR\""
        echo ""
        echo "または、現在のセッションでのみ有効にする場合:"
        echo "  export PATH=\"\$PATH:$INSTALL_DIR\""
        echo ""
        
        # シェル設定ファイルの提案
        local shell_config=""
        case "$SHELL" in
            */bash) shell_config="~/.bashrc または ~/.bash_profile" ;;
            */zsh) shell_config="~/.zshrc" ;;
            */fish) shell_config="~/.config/fish/config.fish" ;;
            *) shell_config="シェル設定ファイル" ;;
        esac
        
        log_info "推奨設定ファイル: $shell_config"
    fi
}

# アンインストール
uninstall_tools() {
    log_info "Unity Toolsをアンインストール中..."
    
    local tools=(
        "unity-compile-check.sh"
        "test-agent-api.sh"
        "examples.sh"
    )
    
    local removed_count=0
    
    for tool in "${tools[@]}"; do
        local target_file="$INSTALL_DIR/$tool"
        
        if [[ -f "$target_file" ]] || [[ -L "$target_file" ]]; then
            rm -f "$target_file"
            log_success "削除: $tool"
            ((removed_count++))
            
            # バックアップファイルも削除
            if [[ -f "$target_file.backup" ]]; then
                rm -f "$target_file.backup"
                log_info "バックアップも削除: $tool.backup"
            fi
        fi
    done
    
    # ドキュメントディレクトリの削除
    local docs_dir="$INSTALL_DIR/unity-tools-docs"
    if [[ -d "$docs_dir" ]]; then
        rm -rf "$docs_dir"
        log_success "ドキュメントディレクトリを削除"
    fi
    
    if [[ $removed_count -eq 0 ]]; then
        log_warning "削除するファイルが見つかりませんでした"
    else
        log_success "アンインストール完了: $removed_count ファイル削除"
    fi
}

# インストール後の動作確認
verify_installation() {
    log_info "インストールを確認中..."
    
    local unity_check_script="$INSTALL_DIR/unity-compile-check.sh"
    
    if [[ -x "$unity_check_script" ]]; then
        # ヘルプ表示でスクリプトが動作するかテスト
        if "$unity_check_script" --help >/dev/null 2>&1; then
            log_success "Unity Compile Check Script: 動作確認OK"
        else
            log_error "Unity Compile Check Script: 動作確認失敗"
            return 1
        fi
    else
        log_error "Unity Compile Check Script が見つかりません"
        return 1
    fi
    
    log_success "すべてのツールが正常にインストールされました"
    return 0
}

# メイン処理
main() {
    local use_symlink=false
    local check_only=false
    local uninstall=false
    
    # 引数解析
    while [[ $# -gt 0 ]]; do
        case $1 in
            --install-dir)
                INSTALL_DIR="$2"
                shift 2
                ;;
            --global)
                INSTALL_DIR="/usr/local/bin"
                shift
                ;;
            --symlink)
                use_symlink=true
                shift
                ;;
            --check)
                check_only=true
                shift
                ;;
            --uninstall)
                uninstall=true
                shift
                ;;
            -h|--help)
                show_help
                exit 0
                ;;
            *)
                log_error "不明なオプション: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    echo "Unity Tools インストーラー v2.0"
    echo "================================="
    echo ""
    
    # チェックのみモード
    if [[ "$check_only" == true ]]; then
        check_dependencies
        check_unity
        exit $?
    fi
    
    # アンインストールモード
    if [[ "$uninstall" == true ]]; then
        uninstall_tools
        exit 0
    fi
    
    # 通常のインストール
    log_info "インストール先: $INSTALL_DIR"
    echo ""
    
    # 事前チェック
    if ! check_dependencies; then
        exit 1
    fi
    
    check_unity
    echo ""
    
    # グローバルインストールの権限チェック
    if [[ "$INSTALL_DIR" == "/usr/local/bin" ]] && [[ $EUID -ne 0 ]]; then
        log_error "グローバルインストールにはroot権限が必要です"
        log_info "以下のいずれかを実行してください:"
        echo "  sudo $0 --global"
        echo "  $0 --install-dir ~/bin"
        exit 1
    fi
    
    # インストール実行
    install_tools "$use_symlink"
    echo ""
    
    # 動作確認
    if verify_installation; then
        echo ""
        check_path
        echo ""
        
        log_success "Unity Tools インストール完了!"
        echo ""
        echo "使用方法:"
        echo "  unity-compile-check.sh --check ."
        echo "  test-agent-api.sh"
        echo "  examples.sh help"
        echo ""
        echo "詳細なドキュメント: $INSTALL_DIR/unity-tools-docs/"
    else
        log_error "インストールに問題があります"
        exit 1
    fi
}

# スクリプトが直接実行された場合のみメイン関数を呼び出し
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi