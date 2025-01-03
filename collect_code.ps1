# 現在のディレクトリ以下のファイルを収集してMarkdownにまとめるスクリプト

# 現在のディレクトリを取得
$repositoryRoot = Get-Location
$outputFile = "$repositoryRoot\ソースコード.md"

# 出力ファイルが既に存在する場合は削除
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# ヘッダーを出力
"# ソースコード" | Out-File -FilePath $outputFile -Encoding utf8

# 収集するファイルの拡張子を指定（例: .cs, .txt）
$allowedExtensions = @(".cs", ".txt", ".csproj", ".props")

# Git管理されているすべてのファイルを取得
$files = git ls-files | Where-Object {
    $extension = [System.IO.Path]::GetExtension($_)
    $allowedExtensions -contains $extension
}

foreach ($file in $files) {
    # ファイルのフルパスを取得
    $filePath = Join-Path $repositoryRoot $file

    # ファイル名をセクションヘッダーとして追加
    Add-Content -Path $outputFile -Value "`n## File: $file`n"

    # コードブロックをMarkdownに追加
    $fileExtension = [System.IO.Path]::GetExtension($filePath).TrimStart('.')
    Add-Content -Path $outputFile -Value "``````$fileExtension"

    # ファイル内容をUTF-8として正しく読み取る
    $fileContent = Get-Content -Path $filePath -Encoding UTF8
    if (-not $fileContent) {
        $fileContent = [System.Text.Encoding]::UTF8.GetString([System.IO.File]::ReadAllBytes($filePath))
    }

    # ファイル内容をMarkdownに追加
    $fileContent | Add-Content -Path $outputFile
    Add-Content -Path $outputFile -Value "``````"
}

Write-Host "Markdownファイルが生成されました: $outputFile"
