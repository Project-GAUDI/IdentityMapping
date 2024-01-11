namespace IdentityMapping
{
    using System;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json.Linq;
    using TICO.GAUDI.Commons;

    class Program
    {
        static IModuleClient MyModuleClient { get; set; } = null;

        static Logger MyLogger { get; } = Logger.GetLogger(typeof(Program));

        static List<RouteInfo> RouteInfos { get; set; } = null;

        static void Main(string[] args)
        {
            try
            {
                Init().Wait();
            }
            catch (Exception e)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Init failed. {e}", true);
                Environment.Exit(1);
            }

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// </summary>
        static async Task Init()
        {
            // 取得済みのModuleClientを解放する
            if (MyModuleClient != null)
            {
                await MyModuleClient.CloseAsync();
                MyModuleClient.Dispose();
                MyModuleClient = null;
            }

            // 環境変数から送信トピックを判定
            TransportTopic defaultSendTopic = TransportTopic.Iothub;
            string sendTopicEnv = Environment.GetEnvironmentVariable("DefaultSendTopic");
            if (Enum.TryParse(sendTopicEnv, true, out TransportTopic sendTopic))
            {
                MyLogger.WriteLog(Logger.LogLevel.INFO, $"Evironment Variable \"DefaultSendTopic\" is {sendTopicEnv}.");
                defaultSendTopic = sendTopic;
            }
            else
            {
                MyLogger.WriteLog(Logger.LogLevel.DEBUG, "Evironment Variable \"DefaultSendTopic\" is not set.");
            }

            // 環境変数から受信トピックを判定
            TransportTopic defaultReceiveTopic = TransportTopic.Iothub;
            string receiveTopicEnv = Environment.GetEnvironmentVariable("DefaultReceiveTopic");
            if (Enum.TryParse(receiveTopicEnv, true, out TransportTopic receiveTopic))
            {
                MyLogger.WriteLog(Logger.LogLevel.INFO, $"Evironment Variable \"DefaultReceiveTopic\" is {receiveTopicEnv}.");
                defaultReceiveTopic = receiveTopic;
            }
            else
            {
                MyLogger.WriteLog(Logger.LogLevel.DEBUG, "Evironment Variable \"DefaultReceiveTopic\" is not set.");
            }

            // MqttModuleClientを作成
            if (Boolean.TryParse(Environment.GetEnvironmentVariable("M2MqttFlag"), out bool m2mqttFlag) && m2mqttFlag)
            {
                string sasTokenEnv = Environment.GetEnvironmentVariable("SasToken");
                MyModuleClient = new MqttModuleClient(sasTokenEnv, defaultSendTopic: defaultSendTopic, defaultReceiveTopic: defaultReceiveTopic);
            }
            // IoTHubModuleClientを作成
            else
            {
                ITransportSettings[] settings = null;
                string protocolEnv = Environment.GetEnvironmentVariable("TransportProtocol");
                if (Enum.TryParse(protocolEnv, true, out TransportProtocol transportProtocol))
                {
                    MyLogger.WriteLog(Logger.LogLevel.INFO, $"Evironment Variable \"TransportProtocol\" is {protocolEnv}.");
                    settings = transportProtocol.GetTransportSettings();
                }
                else
                {
                    MyLogger.WriteLog(Logger.LogLevel.DEBUG, "Evironment Variable \"TransportProtocol\" is not set.");
                }

                MyModuleClient = await IotHubModuleClient.CreateAsync(settings, defaultSendTopic, defaultReceiveTopic).ConfigureAwait(false);
            }

            // edgeHubへの接続
            while (true)
            {
                try
                {
                    await MyModuleClient.OpenAsync().ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    MyLogger.WriteLog(Logger.LogLevel.WARN, $"Open a connection to the Edge runtime is failed. {e.Message}");
                    await Task.Delay(1000);
                }
            }

            // Loggerへモジュールクライアントを設定
            Logger.SetModuleClient(MyModuleClient);

            // 環境変数からログレベルを設定
            string logEnv = Environment.GetEnvironmentVariable("LogLevel");
            try
            {
                if (logEnv != null) Logger.SetOutputLogLevel(logEnv);
                MyLogger.WriteLog(Logger.LogLevel.INFO, $"Output log level is: {Logger.OutputLogLevel.ToString()}");
            }
            catch (ArgumentException e)
            {
                MyLogger.WriteLog(Logger.LogLevel.WARN, $"Environment LogLevel does not expected string. Exception:{e.Message}");
            }

            // desiredプロパティの取得
            var twin = await MyModuleClient.GetTwinAsync().ConfigureAwait(false);
            var collection = twin.Properties.Desired;
            bool isready = false;
            try
            {
                SetMyProperties(collection);
                isready = true;
            }
            catch (Exception e)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"SetMyProperties failed. {e}", true);
                isready = false;
            }

            // プロパティ更新時のコールバックを登録
            await MyModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null).ConfigureAwait(false);

            if (isready)
            {
                // 全ルートの受信時コールバックを登録
                foreach (var info in RouteInfos)
                {
                    await MyModuleClient.SetInputMessageHandlerAsync(info.Input, ReceiveMessage, info).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// プロパティ更新時のコールバック処理
        /// </summary>
        static async Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            MyLogger.WriteLog(Logger.LogLevel.INFO, "OnDesiredPropertiesUpdate Called.");

            try
            {
                await Init();
            }
            catch (Exception e)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"OnDesiredPropertiesUpdate failed. {e}", true);
            }
        }


        /// <summary>
        /// メッセージ受信時のコールバック処理
        /// </summary>
        static async Task<MessageResponse> ReceiveMessage(IotMessage message, object userContext)
        {
            try
            {
                byte[] messageBytes = message.GetBytes();

                if ((int)Logger.OutputLogLevel <= (int)Logger.LogLevel.TRACE)
                {
                    string messageString = Encoding.UTF8.GetString(messageBytes);
                    MyLogger.WriteLog(Logger.LogLevel.TRACE, $"Received Message. Body: [{messageString}]");
                }

                RouteInfo info = (RouteInfo)userContext;

                var pipeMessage = new IotMessage(messageBytes);

                // プロパティの追加・置換
                pipeMessage.SetProperties(info.Properties, IotMessage.PropertySetMode.AddOrModify);
                pipeMessage.SetProperties(message.GetProperties(), IotMessage.PropertySetMode.Add);

                // MessageIdの継承
                pipeMessage.message.MessageId = message.message.MessageId;

                await MyModuleClient.SendEventAsync(info.Output, pipeMessage);
                MyLogger.WriteLog(Logger.LogLevel.DEBUG, "Received message sent");

            }
            catch (Exception e)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"ReceiveMessage failed. {e}", true);
            }

            return MessageResponse.Completed;
        }

        /// <summary>
        /// desiredプロパティから自クラスのプロパティをセットする
        /// </summary>
        static void SetMyProperties(TwinCollection desiredProperties)
        {
            // Routes
            RouteInfos = new List<RouteInfo>();

            // 入力必須チェック
            JObject routes;
            try
            {
                routes = desiredProperties["routes"] as JObject;

            }
            catch (ArgumentOutOfRangeException e)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes dose not exist: {e.ToString()}", true);
                throw;
            }

            // JObjectへのキャスト可否チェック
            if (routes == null)
            {
                MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes is unexpected value.", true);
                throw new ArgumentException();
            }

            // 各routeの値を取得
            foreach (KeyValuePair<string, JToken> route in routes)
            {
                var robj = route.Value as JObject;
                if(robj == null)
                {
                    MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes[{route.Key}] is unexpected value.", true);
                    throw new ArgumentException();
                }

                var itkn = robj["input"] as JValue;
                if(itkn == null)
                {
                    MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes[{route.Key}].input is unexpected value.", true);
                    throw new ArgumentException();
                }
                string input = itkn.Value.ToString();

                var otkn = robj["output"] as JValue;
                if(otkn == null)
                {
                    MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes[{route.Key}].output is unexpected value.", true);
                    throw new ArgumentException();
                }
                string output = otkn.Value.ToString();

                var pobj = robj["add_or_replace"] as JObject;
                if(pobj == null)
                {
                    MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes[{route.Key}].add_or_replace is unexpected value.", true);
                    throw new ArgumentException();
                }

                Dictionary<string, string> properties = new Dictionary<string, string>();
                StringBuilder sb = new StringBuilder("add_or_replace:");
                foreach (var prop in pobj)
                {
                    var val = prop.Value as JValue;
                    if(val == null)
                    {
                        MyLogger.WriteLog(Logger.LogLevel.ERROR, $"Property routes[{route.Key}].add_or_replace[{prop.Key}] is unexpected value.", true);
                        throw new ArgumentException();
                    }
                    properties.Add(prop.Key, val.Value.ToString());
                    sb.AppendLine($"  {prop.Key}: {val.Value}");
                }

                RouteInfos.Add(new RouteInfo(input, output, properties));

                MyLogger.WriteLog(Logger.LogLevel.INFO, $"Property routes[{route.Key}] input:{input}, output:{output}, {sb}");
            }
        }
    }
}
