# IdentityMapping

## 目次
* [概要](#概要)
* [機能](#機能)
* [Quick Start](#quick-start)
* [イメージのURL](#イメージのurl)
* [動作保証環境](#動作保証環境)
* [Deployment 設定値](#deployment-設定値)
  * [環境変数](#環境変数)
  * [Desired Properties](#desired-properties)
  * [Create Option](#create-option)
  * [startupOrder](#startuporder)
* [受信メッセージ](#受信メッセージ)
  * [Message Body](#message-body)
  * [Message Properties](#message-properties)
* [送信メッセージ](#送信メッセージ)
  * [Message Body](#SendMessageBody)
  * [Message Properties](#SendMessageProperties)
* [Direct Method](#direct-method)
  * [SetLogLevel](#setloglevel)
  * [GetLogLevel](#getloglevel)
* [ログ出力内容](#ログ出力内容)
* [ユースケース](#ユースケース)
  * [ケース ①](#Usecase1)
* [Feedback](#feedback)
* [LICENSE](#license)

## 概要
IdentityMappingは、メッセージにプロパティ付与を行うAzure IoT edgeモジュールです。

## 機能

入力されたメッセージに対して任意のプロパティを追加／置換してメッセージを受け渡す。<br>
入力されたメッセージに編集するプロパティが設定されている場合、desiredProperty の値が優先され、置換される。

![schematic diagram](./docs/img/schematic_diagram.drawio.png)

## Quick Start
1. Personal Accese tokenを作成
（参考: [個人用アクセス トークンを管理する](https://docs.github.com/ja/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)）

2. リポジトリをクローン
```
git clone https://github.com/Project-GAUDI/IdentityMapping.git
```

3. ./src/nuget_template.configの<GITHUB_USERNAME>と<PERSONAL_ACCESS_TOKEN>を自身のユーザー名とPersonal Accese tokenに書き換えて、ファイル名をnuget.configに変更してください

4. Dockerイメージをビルド
```
docker image build -t <IMAGE_NAME> ./IdentityMapping/src/
```
例）
```
docker image build -t ghcr.io/<YOUR_GITHUB_USERNAME>/identitymapping:<VERSION> ./IdentityMapping/src/
```

5. Dockerイメージをコンテナレジストリにプッシュ
```
docker push <IMAGE_NAME>
```
例）
```
docker push ghcr.io/<YOUR_GITHUB_USERNAME>/identitymapping:<VERSION>
```

6. Azure IoT edgeで利用

## イメージのURL
準備中
| URL                                                             | Description          |
| --------------------------------------------------------------- | -------------------- |

## 動作保証環境

| Module Version | IoTEdge | edgeAgent | edgeHub  | amd64 verified on | arm64v8 verified on | arm32v7 verified on |
| -------------- | ------- | --------- | -------- | ----------------- | ------------------- | ------------------- |
| 6.0.1          | 1.5.0   | 1.5.6     | 1.5.6    | ubuntu22.04       | －                  | －                  |

## Deployment 設定値

### 環境変数

#### 環境変数の値

| Key                       | Required | Default | Recommend | Description                                                     |
| ------------------------- | -------- | ------- | --------- | ---------------------------------------------------------------- |
| TransportProtocol         |          | Amqp    |           | ModuleClient の接続プロトコル。<br>["Amqp", "Mqtt"] |
| LogLevel                  |          | info    |           | 出力ログレベル。<br>["trace", "debug", "info", "warn", "error"] |

### Desired Properties

#### Desired Properties の値

| JSON Key                                                                  | Type    | Required | Default | Recommend | Description                                                                |
| ------------------------------------------------------------------------- | ------- | -------- | ------- | --------- | ------------------------------------------------------------------------   |
| routes                                                                    | object  | 〇       |         |           | ルートの定義。                                                             |
| &nbsp; route[x]<br>\*キーはなんでもよい(route[x]は重複回避のための推奨値) | object  | 〇       |         |           | [x]は1から始まる連番。<br>プロパティ定義の判別用キー。<br>ログ出力のみで使用。 |
| &nbsp; &nbsp; input                                                       | string  | 〇       |         |           | プロパティを編集するメッセージのインプット名。                             |
| &nbsp; &nbsp; output                                                      | string  | 〇       |         |           | プロパティを編集したメッセージを送信するアウトプット名。                   |
| &nbsp; &nbsp; add_or_replace                                              | object  | 〇       |         |           | 追加／置換するプロパティ情報。                                             |
| &nbsp; &nbsp; &nbsp; key                                                  | string  |          | null    |           | メッセージにプロパティを追加する際のキー。                                 |
| &nbsp; &nbsp; &nbsp; value                                                | string  |          | null    |           | メッセージにプロパティを追加する際の値。                                   |

#### Desired Properties の記入例

```json
{
  "routes": {
    "route1": {
      "input": "in1",
      "output": "output",
      "add_or_replace": {
        "country": "JP",
        "company": "01",
        "factory": "301",
        "data_type": "004",
        "free_area": "30103",
        "format": "007",
        "ProductionMode": "2"
      }
    },
    "route2": {}
  }
}
```

### Create Option

#### Create Option の値

なし

#### Create Option の記入例

```json
{}

```

### startupOrder

#### startupOrder の値

| JSON Key      | Type    | Required | Default | Recommend | Description |
| ------------- | ------- | -------- | ------- | --------- | ----------- |
| startupOrder  | uint    |  | 4294967295 | 400 | モジュールの起動順序。数字が小さいほど先に起動される。<br>["0"から"4294967295"] |

#### startupOrder の記入例

```json
{
  "startupOrder": 400
}
```

## 受信メッセージ

### Message Body

任意

### Message Properties

任意

## 送信メッセージ

<a id="SendMessageBody"></a>

### Message Body

受信したメッセージのbodyをそのまま送信する

<a id="SendMessageProperties"></a>

### Message Properties

| Key                                                 | Description                                           |
| --------------------------------------------------- | ----------------------------------------------------- |
| Desired Properties の add_or_replace で設定した key | Desired Properties の add_or_replace で設定した value |

## Direct Method

### SetLogLevel

* 機能概要

  実行中に一時的にLogLevelを変更する。<br>
  変更はモジュール起動中または有効時間を過ぎるまで有効。<br>

* payload

  | JSON Key      | Type    | Required | default | Description |
  | ------------- | ------- | -------- | -------- | ----------- |
  | EnableSec     | integer  | 〇       |          | 有効時間(秒)。<br>-1:無期限<br>0:リセット(環境変数LogLevel相当に戻る)<br>1以上：指定時間(秒)経過まで有効。  |
  | LogLevel      | string  | △       |          | EnableSec=0以外を指定時必須。指定したログレベルに変更する。<br>["trace", "debug", "info", "warn", "error"]  |

  １時間"trace"レベルに変更する場合の設定例

  ```json
  {
    "EnableSec": 3600,
    "LogLevel": "trace"
  }
  ```

* response

  | JSON Key      | Type    | Description |
  | ------------- | ------- | ----------- |
  | status          | integer | 処理ステータス。<br>0:正常終了<br>その他:異常終了         |
  | payload          | object  | レスポンスデータ。         |
  | &nbsp; CurrentLogLevel | string  | 設定後のログレベル。（正常時のみ）<br>["trace", "debug", "info", "warn", "error"]  |
  | &nbsp; Error | string  | エラーメッセージ（エラー時のみ）  |

  ```json
  {
    "status": 0,
    "paylaod":
    {
      "CurrentLogLevel": "trace"
    }
  }
  ```

### GetLogLevel

* 機能概要

  現在有効なLogLevelを取得する。<br>
  通常は、LogLevel環境変数の設定値が返り、SetLogLevelで設定した有効時間内の場合は、その設定値が返る。<br>

* payload

  なし

* response

  | JSON Key      | Type    | Description |
  | ------------- | ------- | ----------- |
  | status          | integer | 処理ステータス。<br>0:正常終了<br>その他:異常終了         |
  | payload          | object  | レスポンスデータ。         |
  | &nbsp; CurrentLogLevel | string  | 現在のログレベル。（正常時のみ）<br>["trace", "debug", "info", "warn", "error"]  |
  | &nbsp; Error | string  | エラーメッセージ（エラー時のみ）  |

  ```json
  {
    "status": 0,
    "paylaod":
    {
      "CurrentLogLevel": "trace"
    }
  }
  ```

## ログ出力内容

| LogLevel | 出力概要 |
| -------- | -------- |
| error    | [初期化/desired更新/desired取り込み/メッセージ受信]失敗         |
| warn     | エッジランタイムとの接続リトライ失敗<br>環境変数の1部値不正         |
| info     | 環境変数の値<br>desired更新通知<br>環境変数の値未設定のためDefault値適用<br>メッセージ[送信/受信]通知         |
| debug    | 無し     |
| trace    | メソッドの開始・終了<br>受信メッセージBody  |

## ユースケース

<a id="Usecase1"></a>

### ケース①

IoTHubに送信するメッセージにデバイスID等の識別子をメッセージを付与して送信する。

![schematic diagram](./docs/img/usecase_diagram.drawio.png)

#### desiredProperties

```JSON
{
 "routes": {
  "route1": {
   "input": "in1",
   "output": "output",
   "add_or_replace": {
    "country": "JP",
    "company": "01",
    "factory": "301",
    "data_type": "004",
    "free_area": "30103",
    "format": "007",
    "ProductionMode": "2"
   }
  },
  "route2": {}
 }
}
```

#### 環境変数

| 名称     | 値    |
| -------- | ----- |
| LogLevel | debug |

#### 出力結果

CSVReceiverのメッセージにroute1で定義した
プロパティを付与したメッセージ

#### 出力例

```JSON
{
  "RecordHeader": [{"ファイル名"}],
  "RecordData": [{"CSVのレコードデータ"}],
  "Properties": {
    "filename": {"ファイル名"},
    "row_count": {"CSVファイルの行数"},
    "country": "JP",
    "company": "01",
    "factory": "301",
    "data_type": "004",
    "free_area": "30103",
    "format": "007",
    "ProductionMode": "2"
  }
}
```

## Feedback
お気づきの点があれば、ぜひIssueにてお知らせください。

## LICENSE
IdentityMapping is licensed under the MIT License, see the [LICENSE](LICENSE) file for details.
