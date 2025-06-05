# LLM Agent API 仕様書

## 📋 概要

Unity Compile Check ScriptのLLM Agent向けAPI仕様書です。AIエージェントがUnityプロジェクトのコンパイル状況を効率的かつ安全に監視できるように設計されています。

## 🤖 API設計思想

### 安全第一
- **非破壊的**: Unityエディターを終了させることはありません
- **予測可能**: 常に一貫した動作を保証
- **フェイルセーフ**: エラー時も安全な状態を維持

### 機械処理向け
- **構造化出力**: JSON形式での詳細な結果
- **明確な戻り値**: プログラムで判定しやすい終了コード
- **静粛モード**: 不要な出力を抑制

## 🚀 基本API

### 1. コンパイルチェック API
```bash
./unity-compile-check.sh --check <project_path>
```

**用途**: 最も安全なコンパイルエラーチェック  
**特徴**: Unityエディターに影響なし  
**推奨度**: ⭐⭐⭐⭐⭐

#### 戻り値
- `0`: コンパイル成功
- `1`: コンパイルエラーあり
- `2`: スクリプト実行エラー

#### 出力例
```
📁 Project: .
⚙️  Editor log check only
🔍 Checking Unity Editor logs...
✅ Compilation successful
✅ Compilation check passed
```

### 2. JSON API
```bash
./unity-compile-check.sh --json --check <project_path>
```

**用途**: 機械処理向けの構造化データ取得  
**特徴**: パース可能なJSON出力  
**推奨度**: ⭐⭐⭐⭐⭐

#### 成功時のJSON
```json
{
  "success": true,
  "compilation": {
    "status": "success",
    "errors": [],
    "error_count": 0,
    "project_path": ".",
    "timestamp": "2025-06-05T14:30:00Z"
  }
}
```

#### エラー時のJSON
```json
{
  "success": false,
  "compilation": {
    "status": "failed",
    "errors": [
      "Assets/Scripts/Test.cs(10,5): error CS1002: ; expected",
      "Assets/Scripts/Main.cs(25,10): error CS0103: 'undefined' does not exist"
    ],
    "error_count": 2,
    "project_path": ".",
    "timestamp": "2025-06-05T14:30:00Z"
  }
}
```

#### システムエラー時のJSON
```json
{
  "success": false,
  "error": "missing_project_path",
  "message": "Unity project path is required"
}
```

### 3. シンプル API
```bash
./unity-compile-check.sh --simple --check <project_path>
```

**用途**: 成功/失敗の簡単な判定  
**特徴**: 1行での結果出力  
**推奨度**: ⭐⭐⭐⭐

#### 出力例
```
SUCCESS
```
または
```
FAILED (3 errors)
```

### 4. コンパイルトリガー API
```bash
./unity-compile-check.sh --trigger <project_path>
```

**用途**: Unityでの再コンパイル実行後チェック  
**特徴**: AppleScript経由でUnityに再コンパイル指示  
**推奨度**: ⭐⭐⭐

## 🔧 実践的な使用例

### 例1: 基本的なエラーチェック
```bash
#!/bin/bash

check_unity_compilation() {
    local project_path="$1"
    
    # シンプルな成功/失敗チェック
    result=$(./unity-compile-check.sh --simple --check "$project_path" 2>/dev/null)
    
    if [[ "$result" == "SUCCESS" ]]; then
        echo "✅ コンパイル成功"
        return 0
    else
        echo "❌ コンパイルエラー: $result"
        return 1
    fi
}

# 使用例
if check_unity_compilation "."; then
    echo "次の処理に進む"
else
    echo "エラーを修正してください"
fi
```

### 例2: JSON解析による詳細処理
```bash
#!/bin/bash

analyze_compilation_errors() {
    local project_path="$1"
    
    # JSON形式で詳細情報を取得
    json_result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    
    # jqでJSONを解析
    success=$(echo "$json_result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "✅ コンパイル成功"
        return 0
    else
        # エラー詳細を表示
        error_count=$(echo "$json_result" | jq -r '.compilation.error_count')
        echo "❌ エラー数: $error_count"
        
        # 各エラーメッセージを表示
        echo "$json_result" | jq -r '.compilation.errors[]' | while read -r error; do
            echo "  - $error"
        done
        
        return 1
    fi
}

# 使用例
analyze_compilation_errors "."
```

### 例3: 戻り値による分岐処理
```bash
#!/bin/bash

handle_compilation_result() {
    local project_path="$1"
    
    # コンパイルチェック実行
    ./unity-compile-check.sh --check "$project_path"
    local exit_code=$?
    
    case $exit_code in
        0)
            echo "✅ コンパイル成功 - 次の処理を実行"
            deploy_build
            ;;
        1)
            echo "❌ コンパイルエラー - 詳細を取得"
            get_error_details "$project_path"
            ;;
        2)
            echo "🔥 スクリプト実行エラー - システムを確認"
            check_system_requirements
            ;;
    esac
}

get_error_details() {
    local project_path="$1"
    echo "エラー詳細:"
    ./unity-compile-check.sh --json --check "$project_path" 2>/dev/null | \
        jq -r '.compilation.errors[]'
}
```

### 例4: 継続的監視
```bash
#!/bin/bash

monitor_unity_project() {
    local project_path="$1"
    local check_interval="${2:-30}"  # デフォルト30秒間隔
    
    echo "Unity プロジェクト監視開始 (間隔: ${check_interval}秒)"
    
    while true; do
        timestamp=$(date '+%Y-%m-%d %H:%M:%S')
        result=$(./unity-compile-check.sh --simple --check "$project_path" 2>/dev/null)
        
        echo "[$timestamp] $result"
        
        # エラーが発生した場合の詳細ログ
        if [[ "$result" != "SUCCESS" ]]; then
            ./unity-compile-check.sh --json --check "$project_path" 2>/dev/null | \
                jq -r '.compilation.errors[]' | \
                sed 's/^/  ERROR: /'
        fi
        
        sleep "$check_interval"
    done
}

# 使用例: プロジェクトを30秒間隔で監視
monitor_unity_project "." 30
```

## 📊 エラーハンドリング

### エラータイプと対応

#### 1. プロジェクトパスエラー
```json
{
  "success": false,
  "error": "missing_project_path",
  "message": "Unity project path is required"
}
```

**対応**: 正しいプロジェクトパスを指定

#### 2. プロジェクト未検出エラー
```json
{
  "success": false,
  "error": "project_not_found",
  "message": "Project directory does not exist: /invalid/path"
}
```

**対応**: パスの存在確認

#### 3. 無効なUnityプロジェクト
```json
{
  "success": false,
  "error": "invalid_unity_project",
  "message": "Not a valid Unity project: /path"
}
```

**対応**: ProjectSettings/ProjectVersion.txtの存在確認

#### 4. ログファイル未検出
```json
{
  "success": false,
  "error": "no_log_files",
  "message": "No Unity log files found"
}
```

**対応**: Unity起動またはログディレクトリ確認

### エラーハンドリング例
```bash
#!/bin/bash

safe_unity_check() {
    local project_path="$1"
    local json_result
    
    # JSON形式で結果を取得
    json_result=$(./unity-compile-check.sh --json --check "$project_path" 2>/dev/null)
    local exit_code=$?
    
    # JSONの解析に失敗した場合
    if ! echo "$json_result" | jq . >/dev/null 2>&1; then
        echo "JSON解析エラー: 不正な出力"
        return 2
    fi
    
    # success フィールドをチェック
    local success=$(echo "$json_result" | jq -r '.success')
    
    if [[ "$success" == "true" ]]; then
        echo "コンパイル成功"
        return 0
    elif [[ "$success" == "false" ]]; then
        # エラータイプを確認
        local error_type=$(echo "$json_result" | jq -r '.error // empty')
        
        if [[ -n "$error_type" ]]; then
            # システムエラー
            local message=$(echo "$json_result" | jq -r '.message')
            echo "システムエラー [$error_type]: $message"
            return 2
        else
            # コンパイルエラー
            local error_count=$(echo "$json_result" | jq -r '.compilation.error_count')
            echo "コンパイルエラー: ${error_count}個のエラー"
            return 1
        fi
    else
        echo "不明なレスポンス"
        return 2
    fi
}
```

## 🔄 CI/CD統合

### GitHub Actions例
```yaml
name: Unity Compile Check

on: [push, pull_request]

jobs:
  compile-check:
    runs-on: macos-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Unity Tools
      run: |
        chmod +x unity-tools/unity-compile-check.sh
        
    - name: Unity Compile Check
      run: |
        cd ${{ github.workspace }}
        result=$(./unity-tools/unity-compile-check.sh --json --check . 2>/dev/null)
        echo "$result" > compile-result.json
        
        success=$(echo "$result" | jq -r '.success')
        if [[ "$success" != "true" ]]; then
          echo "コンパイルエラーが検出されました"
          echo "$result" | jq '.compilation.errors'
          exit 1
        fi
        
    - name: Upload Results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: compile-results
        path: compile-result.json
```

### Jenkins Pipeline例
```groovy
pipeline {
    agent any
    
    stages {
        stage('Unity Compile Check') {
            steps {
                script {
                    sh 'chmod +x unity-tools/unity-compile-check.sh'
                    
                    def result = sh(
                        script: './unity-tools/unity-compile-check.sh --json --check .',
                        returnStdout: true
                    ).trim()
                    
                    def jsonResult = readJSON text: result
                    
                    if (!jsonResult.success) {
                        error "Unity compilation failed: ${jsonResult.compilation.error_count} errors"
                    }
                    
                    echo "Unity compilation successful"
                }
            }
        }
    }
}
```

## 🎯 ベストプラクティス

### 1. 出力形式の選択
- **自動化**: JSON形式を使用
- **簡単な判定**: Simple形式を使用
- **デバッグ**: 標準形式を使用

### 2. エラー処理
- **戻り値チェック**: 必ずexit codeを確認
- **JSON検証**: jqでパース可能か確認
- **タイムアウト**: 長時間実行を避ける

### 3. パフォーマンス
- **stderr抑制**: `2>/dev/null`で不要な出力を除去
- **並列実行**: 複数プロジェクトでの同時実行を避ける
- **キャッシュ**: 短時間での連続実行を制限

### 4. セキュリティ
- **パス検証**: プロジェクトパスのサニタイズ
- **権限確認**: 実行権限の事前チェック
- **ログ保護**: 機密情報の出力回避

## 📈 パフォーマンスメトリクス

### 実行時間
- **--check**: 通常1-2秒
- **--trigger**: 通常4-6秒（コンパイル待機含む）
- **--json**: 標準と同等
- **--simple**: 標準より若干高速

### リソース使用量
- **CPU**: 最小限（ログ解析のみ）
- **メモリ**: < 50MB
- **ディスク**: 読み取り専用
- **ネットワーク**: 不使用

### 制限事項
- **同時実行**: 同一プロジェクトでの並列実行は非推奨
- **ファイルロック**: UnityLockfile検出時の自動切り替え
- **プラットフォーム**: macOS最適化（Linux対応）

## 🔮 将来の拡張予定

### v3.0予定機能
- **WebHook対応**: エラー検出時の自動通知
- **メトリクス収集**: 統計情報の蓄積
- **プラグイン機能**: カスタムエラーパターン

### API拡張
- **リアルタイム監視**: ファイル変更検出
- **複数プロジェクト**: バッチ処理対応
- **REST API**: HTTP経由でのアクセス