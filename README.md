# MyChatBot - AI チャットボットアプリケーション

.NET 10 + Semantic Kernel + Gemini API を使用したチャットボットアプリケーションです。

## 機能

- ✅ OpenAI API / Gemini API のサポート
- ✅ ストリーミング応答（リアルタイムでテキストを表示）
- ✅ コンソールアプリケーション
- ✅ Web インターフェース（SignalR使用）
- ✅ チャット履歴管理

## プロジェクト構成

- **MyChatBot** - コンソールアプリケーション
- **MyChatBot.Web** - Web アプリケーション（ASP.NET Core + SignalR）

## セットアップ

### 1. APIキーの取得

#### OpenAI API
1. https://platform.openai.com/ にアクセス
2. アカウントを作成してログイン
3. API Keysページで新しいキーを作成
4. キーをコピー（再表示不可なので注意！）

#### Gemini API
1. https://ai.google.dev/ にアクセス
2. Googleアカウントでログイン
3. "Get API Key" をクリック
4. APIキーを作成してコピー

### 2. 設定ファイルの編集

`appsettings.json` を編集してAPIキーを設定：

```json
{
  "Provider": "Gemini",
  "OpenAI": {
    "ApiKey": "あなたのOpenAI APIキー",
    "ModelId": "gpt-4o"
  },
  "Gemini": {
    "ApiKey": "あなたのGemini APIキー",
    "ModelId": "gemini-2.0-flash-exp"
  }
}
```

`Provider` を `"OpenAI"` または `"Gemini"` に設定してプロバイダーを切り替えられます。

## 使い方

### コンソールアプリケーション

```bash
cd MyChatBot
dotnet run
```

- メッセージを入力してEnterキーで送信
- `exit` または `quit` で終了

### Web アプリケーション

```bash
cd MyChatBot.Web
dotnet run
```

ブラウザで `http://localhost:5000` または `https://localhost:5001` を開く

## 利用可能なモデル

### OpenAI
- gpt-4o（推奨）
- gpt-4o-mini
- gpt-4-turbo
- gpt-3.5-turbo

### Gemini
- gemini-2.0-flash-exp（推奨・最新）
- gemini-1.5-pro
- gemini-1.5-flash

## 料金について

どちらのAPIも従量課金制です。

- **OpenAI**: gpt-4o は入力 $2.50/1M tokens、出力 $10.00/1M tokens
- **Gemini**: gemini-2.0-flash-exp は無料枠あり（詳細はGoogle AI公式サイトで確認）

## トラブルシューティング

### "APIキーが設定されていません" エラー
- `appsettings.json` にAPIキーが正しく設定されているか確認
- キーの前後にスペースがないか確認

### Web アプリケーションが起動しない
- ポート5000/5001が使用中でないか確認
- `MyChatBot.Web/appsettings.json` にもAPIキーが設定されているか確認

### 応答が返ってこない
- インターネット接続を確認
- APIキーが有効か確認
- API使用量が上限に達していないか確認

## 技術スタック

- .NET 10
- Microsoft Semantic Kernel
- Mscc.GenerativeAI
- ASP.NET Core
- SignalR
- HTML/CSS/JavaScript

## ライセンス

MIT License
