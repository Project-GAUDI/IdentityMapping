using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TICO.GAUDI.Commons;

namespace IotedgeV2IdentityMapping
{
    /// <summary>
    /// Application Main class
    /// </summary>
    internal class MyApplicationMain : IApplicationMain
    {
        static ILogger MyLogger { get; } = LoggerFactory.GetLogger(typeof(MyApplicationMain));
        static List<RouteInfo> RouteInfos { get; set; } = null;
        private int _disposed = 0; // 0: false, 1: true
        /// <summary>
        /// リソース解放
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: DisposeAsync");

            // 2重実行防止
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: Already disposed, skipping DisposeAsync");
                return;
            }

            try
            {
                // ここでApplicationMainのリソース解放処理を行う
                // 非同期リソースがある場合は適切に解放する
                // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝

                await Task.CompletedTask; // 非同期リソース解放処理をここに記述

                // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝
            }
            catch (Exception ex)
            {
                MyLogger.WriteLog(ILogger.LogLevel.ERROR, $"Error in DisposeAsync: {ex.Message}", true);
                // 例外は再スローしない（Disposeでは通常例外を抑制）
            }

            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: DisposeAsync");
        }

        /// <summary>
        /// アプリケーション初期化					
        /// システム初期化前に呼び出される
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitializeAsync()
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: InitializeAsync");

            // ここでApplicationMainの初期化処理を行う。
            // 通信は未接続、DesiredPropertiesなども未取得の状態
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝
            bool retStatus = true;

            await Task.CompletedTask;
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝

            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: InitializeAsync");
            return retStatus;
        }

        /// <summary>
        /// アプリケーション起動処理					
        /// システム初期化完了後に呼び出される
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: StartAsync");

            // ここでApplicationMainの起動処理を行う。
            // 通信は接続済み、DesiredProperties取得済みの状態
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝
            bool retStatus = true;
            // 全ルートの受信時コールバックを登録
            IApplicationEngine appEngine = ApplicationEngineFactory.GetEngine();
            foreach (var info in RouteInfos)
            {
                await appEngine.AddMessageInputHandlerAsync(info.Input, OnMessageReceivedAsync, info).ConfigureAwait(false);
            }
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝

            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: StartAsync");
            return retStatus;
        }

        /// <summary>
        /// アプリケーション解放。					
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TerminateAsync()
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: TerminateAsync");

            // ここでApplicationMainの終了処理を行う。
            // アプリケーション終了時や、
            // DesiredPropertiesの更新通知受信後、
            // 通信切断時の回復処理時などに呼ばれる。
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝
            bool retStatus = true;

            await Task.CompletedTask;
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝

            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: TerminateAsync");
            return retStatus;
        }


        /// <summary>
        /// DesiredPropertis更新コールバック。					
        /// </summary>
        /// <param name="desiredProperties">DesiredPropertiesデータ。JSONのルートオブジェクトに相当。</param>
        /// <returns></returns>
        public async Task<bool> OnDesiredPropertiesReceivedAsync(JObject desiredProperties)
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: OnDesiredPropertiesReceivedAsync");

            // DesiredProperties更新時の反映処理を行う。
            // 必要に応じて、メンバ変数への格納等を実施。
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝
            bool retStatus = true;
            // Routes
            RouteInfos = new List<RouteInfo>();
            // 入力必須チェック
            JObject routes = null;
            try
            {
                routes = Util.GetDesiredProperty<JObject>(desiredProperties, "routes", true);

                if (routes.Count == 0)
                {
                    var errmsg = $"Property routes is empty.";
                    MyLogger.WriteLog(ILogger.LogLevel.ERROR, errmsg, true);
                    MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {errmsg}");
                    retStatus = false;
                    return retStatus;
                }

                // 各routeの値を取得
                foreach (KeyValuePair<string, JToken> route in routes)
                {
                    if(route.Value.Type == JTokenType.Null){
                        var errmsg = $"Property {route.Key} is null.";
                        MyLogger.WriteLog(ILogger.LogLevel.ERROR, errmsg, true);
                        MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {errmsg}");
                        retStatus = false;
                        return retStatus;
                    }

                    var robj = route.Value as JObject;

                    if(robj == null){
                        var errmsg = $"Property {route.Key} is unexpected value.";
                        MyLogger.WriteLog(ILogger.LogLevel.ERROR, errmsg, true);
                        MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {errmsg}");
                        retStatus = false;
                        return retStatus;
                    }

                    JValue itkn = null;

                    itkn = Util.GetDesiredProperty<JValue>(robj, "input", true);


                    string input = itkn.Value.ToString();
                    JValue otkn = null;

                    otkn = Util.GetDesiredProperty<JValue>(robj, "output", true);

                    string output = otkn.Value.ToString();
                    JObject pobj = null;

                    pobj = Util.GetDesiredProperty<JObject>(robj, "add_or_replace", true);


                    Dictionary<string, string> properties = new Dictionary<string, string>();
                    StringBuilder sb = new StringBuilder("add_or_replace:");
                    if (pobj.Count == 0)
                    {
                        var errmsg = $"Property routes[{route.Key}].add_or_replace is empty.";
                        MyLogger.WriteLog(ILogger.LogLevel.ERROR, errmsg, true);
                        MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {errmsg}");
                        retStatus = false;
                        return retStatus;
                    }
                    foreach (var prop in pobj)
                    {
                        var val = prop.Value as JValue;
                        if (val == null || val.Value == null)
                        {
                            var errmsg = $"Property routes[{route.Key}].add_or_replace[{prop.Key}] is unexpected value.";
                            MyLogger.WriteLog(ILogger.LogLevel.ERROR, errmsg, true);
                            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {errmsg}");
                            retStatus = false;
                            return retStatus;
                        }
                        properties.Add(prop.Key, val.Value.ToString());
                        sb.AppendLine($"  {prop.Key}: {val.Value}");
                    }

                    RouteInfos.Add(new RouteInfo(input, output, properties));

                    MyLogger.WriteLog(ILogger.LogLevel.INFO, $"Property routes[{route.Key}] input:{input}, output:{output}, {sb}");
                }
            }
            catch (Exception ex)
            {
                MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Exit Method: OnDesiredPropertiesReceivedAsync caused by {ex.Message}");
                retStatus = false;
                return retStatus;
            }
            await Task.CompletedTask;
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝

            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: OnDesiredPropertiesReceivedAsync");

            return retStatus;
        }

        /// <summary>
        /// メッセージ受信コールバック。					
        /// </summary>
        /// <param name="inputName"></param>
        /// <param name="message"></param>
        /// <param name="userContext"></param>
        /// <returns>
        /// 受信処理成否
        ///     true : 処理成功。
        ///     false ： 処理失敗。edgeHubから再送を受ける。
        /// </returns>
        public async Task<bool> OnMessageReceivedAsync(string inputName, IotMessage message, object userContext)
        {
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Start Method: OnMessageReceivedAsync");

            // メッセージ受信時のコールバック処理を行う。
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここから＝＝＝＝＝＝＝＝＝＝＝＝＝
            bool retStatus = true;

            try
            {
                IJsonSerializer serializer = JsonSerializerFactory.GetJsonSerializer();
                IDictionary<string, string> properties = message.GetProperties();

                byte[] messageBytes = message.GetBytes();

                MyLogger.WriteLog(ILogger.LogLevel.INFO, "1 message received.");

                // 不要な処理を回避の為、TRACEログを出力する設定か確認
                if (MyLogger.IsLogLevelToOutput(ILogger.LogLevel.TRACE))
                {
                    string messageString = Encoding.UTF8.GetString(messageBytes);
                    string serializedProperties = serializer.Serialize<IDictionary<string, string>>(properties);
                    MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Received Message. Body: [{messageString}]");
                    MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Properties: [{serializedProperties}]");
                }
                RouteInfo info = (RouteInfo)userContext;

                // メッセージを複製
                var pipeMessage = new IotMessage(messageBytes);

                // プロパティの追加・置換
                // 本来は既存プロパティ・追加プロパティの設定順であるべきのため、将来的に設定順の修正を実施予定
                // エラーチェックを行っていないため動作結果は期待通りになっているが本来はエラーチェックすべき
                pipeMessage.SetProperties(info.Properties, IotMessage.PropertySetMode.AddOrModify);
                pipeMessage.SetProperties(properties, IotMessage.PropertySetMode.Add);

                string ModifiedProperties = serializer.Serialize<IDictionary<string, string>>(pipeMessage.GetProperties());
                MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"Modified Properties: [{ModifiedProperties}]");

                // MessageIdの継承
                pipeMessage.SetMessageId(message.GetMessageId());

                // メッセージを送信
                IApplicationEngine appEngine = ApplicationEngineFactory.GetEngine();
                await appEngine.SendMessageAsync(info.Output, pipeMessage);
                MyLogger.WriteLog(ILogger.LogLevel.INFO, "1 message sent");
                retStatus = true;

            }
            catch (Exception ex)
            {
                MyLogger.WriteLog(ILogger.LogLevel.ERROR, $"OnMessageReceivedAsync failed. {ex}", true);
                retStatus = false;
            }
            // ＝＝＝＝＝＝＝＝＝＝＝＝＝ここまで＝＝＝＝＝＝＝＝＝＝＝＝＝

            MyLogger.WriteLog(ILogger.LogLevel.DEBUG, $"Return status : {retStatus}");
            MyLogger.WriteLog(ILogger.LogLevel.TRACE, $"End Method: OnMessageReceivedAsync");
            return retStatus;
        }
    }
}