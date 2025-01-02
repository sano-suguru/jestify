# Jestify

[日本語](README.md) | [English](docs/README.en.md)

C#でJestの書き味を実現したテスティングフレームワークです。  
冗長な命名を廃したJest風の直感的な構文で、スマートなテストコードが書けます。

## シンプルで直感的なAPI

```csharp
await Describe("電卓", async () =>
{
    var calculator = new Calculator();

    BeforeEach(() =>
    {
        calculator.Reset();
    });

    await It("足し算ができること", () =>
    {
        var result = calculator.Add(2, 3);
        Assert.Equal(5, result);
    });
});
```

## 特徴

- **Jest風の構文**：`Describe`/`It`による直感的なテスト記述
- **シンプルなAPI**：余計な装飾を排除したミニマルな設計
- **強力な非同期サポート**：async/awaitを完全サポート
- **充実したライフサイクルフック**：`BeforeAll`/`AfterAll`/`BeforeEach`/`AfterEach`
- **データ駆動テスト**：`Each`による繰り返しテストの簡潔な記述
- **モダンな機能**：タイムアウト制御、キャンセレーション、並列実行
- **見やすい出力**：色付きコンソール出力でテスト結果を視覚的に確認

## なぜJestify？

従来のC#テスティングフレームワークと異なり、Jestifyは：

- **余計な属性を排除**：`[TestMethod]`や`[TestClass]`が不要
- **直感的な構造化**：`Describe`によるテストのグループ化が簡単
- **シンプルな非同期**：特別な属性なしでasync/awaitが使える
- **柔軟なセットアップ**：フックによる柔軟なテスト環境の構築

## インストール

```bash
dotnet add package jestify
```

## 実用的なコード例

### ライフサイクルフックの活用

```csharp
await Describe("データベーステスト", async () =>
{
    Database db = null!;

    BeforeAll(async () =>
    {
        db = await Database.CreateAsync();
        await db.MigrateAsync();
    });

    AfterAll(async () =>
    {
        await db.DisposeAsync();
    });

    BeforeEach(async () =>
    {
        await db.ClearAsync();
    });

    await It("データを保存できること", async () =>
    {
        await db.SaveAsync(new TestData());
        Assert.Equal(1, await db.CountAsync());
    });
});
```

### データ駆動テスト

```csharp
await Each(
    "異なるケースをスマートにテスト: {0}",
    new[] { 1, 2, 3, 4, 5 },
    async (number, ct) => 
    {
        var result = await Calculate(number);
        Assert.True(result > 0);
    }
);
```

### 並列テスト

```csharp
await Each(
    "並列テスト {0}",
    Enumerable.Range(1, 100),
    async (number, ct) => 
    {
        await Task.Delay(100, ct);
        Assert.True(number > 0);
    },
    parallel: true  // 並列実行がシンプルに指定可能
);
```

### タイムアウトとスキップ

```csharp
// グローバルタイムアウトの設定
SetTimeout(10000); // 10秒

// 条件付きスキップ
if (!isDatabaseAvailable)
{
    Skip("データベーステスト", "DB接続が利用できません");
}
```

## テストのベストプラクティス

1. **わかりやすいテストタイトル**
   - テストの目的が一目でわかるタイトルを付ける
   - 期待される結果をタイトルに含める

2. **テストの独立性**
   - 各テストは他のテストに依存しない
   - BeforeEachで毎回クリーンな状態を作る

3. **リソース管理**
   - AfterAll/AfterEachで確実にクリーンアップ
   - using文やDisposeパターンを活用

4. **非同期処理**
   - awaitを適切に使用
   - キャンセレーショントークンを活用

5. **パフォーマンス**
   - 長時間テストには適切なタイムアウトを設定
   - 必要に応じて並列実行を活用

## コントリビューション

貢献をお待ちしています！Pull Requestを気軽に送ってください。

## ライセンス

MITライセンス - 詳細は[LICENSE](LICENSE)ファイルをご覧ください。
