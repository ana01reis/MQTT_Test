using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Serilog;

namespace Magent
{
    public class mqttPUB
    {
        public static Dictionary<string, string> SubscribedValues { get; } = new Dictionary<string, string>();

        public mqttPUB(string name)
        {
            this.name = name;

            outToMQTT = new List<object[]>();

            _mqttClient = new MqttFactory().CreateManagedMqttClient();

            options = new ManagedMqttClientOptions();

            lastmsg = new Dictionary<object, object>();
        }

        public void startMQTTclient(string IP, int Port, string clientID, bool mqttUseCert, List<X509Certificate>? cert)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            if (IP == null || Port == 0)
            {
                Console.WriteLine("Missing MQTT config!!!");
                return;
            }

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(clientID)
                .WithTcpServer(IP, Port);

            if (mqttUseCert)
            {
                var tlsOptions = new MqttClientOptionsBuilderTlsParameters
                {
                    Certificates = cert,
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                    UseTls = true,
                    AllowUntrustedCertificates = true,
                    CertificateValidationHandler = delegate { return true; }
                };

                builder = new MqttClientOptionsBuilder()
                    .WithClientId(clientID)
                    .WithTcpServer(IP, Port)
                    .WithTls(tlsOptions);
            }

            options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                .WithClientOptions(builder.Build())
                .Build();

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            _mqttClient.ApplicationMessageReceivedHandler =
                new MqttApplicationMessageReceivedHandlerDelegate(a =>
                {
                    try
                    {
                        var topic = a.ApplicationMessage.Topic;
                        var payload = Encoding.UTF8.GetString(a.ApplicationMessage.Payload ?? Array.Empty<byte>());

                        SubscribedValues[topic] = payload;

                        //Console.WriteLine($"[SUB] {topic} → {payload}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SUB] Erro ao processar mensagem: {ex.Message}");
                    }
                });
        }

        public void startMQTTclient(string IP, int Port, string clientID)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            if (IP == null || Port == 0)
            {
                Console.WriteLine("Missing MQTT config!!!");
                return;
            }

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(clientID)
                .WithTcpServer(IP, Port);

            options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                .WithClientOptions(builder.Build())
                .Build();

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            _mqttClient.ApplicationMessageReceivedHandler =
                new MqttApplicationMessageReceivedHandlerDelegate(a =>
                {
                    try
                    {
                        var topic = a.ApplicationMessage.Topic;
                        var payload = Encoding.UTF8.GetString(a.ApplicationMessage.Payload ?? Array.Empty<byte>());

                        SubscribedValues[topic] = payload;

                        //Console.WriteLine($"[SUB] {topic} → {payload}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SUB] Erro ao processar mensagem: {ex.Message}");
                    }
                });
        }

        public async Task Subscribe(string topic)
        {
            if (!_mqttClient.IsStarted)
            {
                await _mqttClient.StartAsync(options);
                Console.WriteLine("MQTT Client Started (para Subscribe)!");
            }

            var filter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

            await _mqttClient.SubscribeAsync(filter);
            //Console.WriteLine($"[MQTT] Subscrito a: {topic}");
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_mqttClient != null)
                {
                    //Console.WriteLine("[MQTT] A desligar cliente...");

                    // 1) Limpar handlers para evitar callbacks/reconexões
                    _mqttClient.ConnectedHandler = null;
                    _mqttClient.DisconnectedHandler = null;
                    _mqttClient.ConnectingFailedHandler = null;
                    _mqttClient.ApplicationMessageReceivedHandler = null;

                    // 2) Parar o cliente, se estiver ativo
                    if (_mqttClient.IsStarted)
                    {
                        await _mqttClient.StopAsync();
                        //Console.WriteLine("[MQTT] StopAsync concluído.");
                    }

                    // 3) (Opcional) Libertar recursos do cliente
                    //    Na tua versão só existe Dispose() síncrono
                    if (_mqttClient is IDisposable disp)
                    {
                        disp.Dispose();
                        //Console.WriteLine("[MQTT] Dispose concluído.");
                    }

                    // 4) Opcional: marcar como destruído
                    // _mqttClient = null!;
                    // options = null!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao desconectar MQTT: {ex.Message}");
            }
        }

        public void EndMQTTclient(string IP, int Port, string clientID)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            if (IP == null || Port == 0)
            {
                Console.WriteLine("Missing MQTT config!!!");
                return;
            }

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(clientID)
                .WithTcpServer(IP, Port);

            options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                .WithClientOptions(builder.Build())
                .Build();

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            _mqttClient.ApplicationMessageReceivedHandler =
                new MqttApplicationMessageReceivedHandlerDelegate(a =>
                {
                    try
                    {
                        var topic = a.ApplicationMessage.Topic;
                        var payload = Encoding.UTF8.GetString(a.ApplicationMessage.Payload ?? Array.Empty<byte>());

                        SubscribedValues[topic] = payload;

                        //Console.WriteLine($"[SUB] {topic} → {payload}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SUB] Erro ao processar mensagem: {ex.Message}");
                    }
                });
        }

        public void pubTopic(List<object[]> msg, string deviceType, string DeviceID, string manufacturerName, string serialNo, string beaconMsg)
        {
            if (!_mqttClient.IsStarted)
            {
                _mqttClient.StartAsync(options).GetAwaiter().GetResult();

                Console.WriteLine("MQTT Client Connected!!!");

                string jsonBeacon =
                    @"{ ""Connection Message"": """ + beaconMsg + " This is " +
                    _mqttClient.Options.ClientOptions.ClientId +
                    " connecting at " + DateTimeOffset.UtcNow + @""",";

                string jsonInfo = jsonBeacon + @" ""AgentVersion"": " + 4 + @" }";

                _mqttClient.PublishAsync(deviceType + "/" + DeviceID + "/devicesetup", jsonInfo);

                string jsonAtrib =
                    @"{ ""manufacturer"" : """ + manufacturerName + @""",  ""serialNo"" : """ +
                    serialNo + @""" , ""lastConnectionTime"" : " +
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}";

                Console.WriteLine(jsonAtrib);
                _mqttClient.PublishAsync(deviceType + "/" + DeviceID + "/atributes", jsonAtrib);
            }

            var json = @"{ ";

            foreach (var singmsg in msg)
            {
                bool checker = false;

                try
                {
                    if (Convert.ToBoolean(lastmsg[singmsg[0]]) == Convert.ToBoolean(singmsg[1]))
                        checker = true;
                }
                catch { }

                try
                {
                    if (Convert.ToDouble(lastmsg[singmsg[0]]) == Convert.ToDouble(singmsg[1]))
                        checker = true;
                }
                catch { }

                if (checker)
                {
                    Console.WriteLine($"{singmsg[0]} did not changed since last message sent!");
                }
                else
                {
                    try
                    {
                        try
                        {
                            json += @"""" + singmsg[0] + @""" :";
                            if (Convert.ToString(singmsg[1]) == "False") singmsg[1] = "false";
                            if (Convert.ToString(singmsg[1]) == "True") singmsg[1] = "true";
                            json += singmsg[1] + ",";
                        }
                        catch
                        {
                            json += @"""" + singmsg[0] + @""" :";
                            json += Convert.ToString(singmsg[1]) + ",";
                        }

                        Console.WriteLine($"{singmsg[0]} value is {singmsg[1]}");

                        lastmsg[singmsg[0]] = singmsg[1];
                    }
                    catch
                    {
                        lastmsg.Add(singmsg[0], singmsg[1]);
                    }
                }
            }

            json += @"""MQTTtimeStamp"": " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}";

            _mqttClient.PublishAsync(deviceType + "/" + DeviceID + "/features", json);

            Console.WriteLine(" \n Topic:" + deviceType + "/" + DeviceID + "/features");
            Console.WriteLine(" \n Message" + json);
            Console.WriteLine(" at " + DateTimeOffset.UtcNow + " \n ");

            outToMQTT.Clear();
        }

        public void OnConnected(MqttClientConnectedEventArgs obj)
        {
            //Log.Logger.Information("Successfully connected.");
        }

        public void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            //Log.Logger.Warning("Couldn't connect to broker.");
        }

        public void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            //Log.Logger.Information("Successfully disconnected.");
        }

        public static long getUTCTimeInMilliseconds(DateTime utcDT)
        {
            DateTime convertedDate = DateTime.SpecifyKind(utcDT, DateTimeKind.Utc);
            return new DateTimeOffset(convertedDate).ToUnixTimeMilliseconds();
        }

        public List<object[]> mqttMsgUpdate(List<object[]> inboundMSG, string? prefix)
        {
            foreach (object[] objMsg in inboundMSG)
            {
                outToMQTT.Add(objMsg);
            }

            return outToMQTT;
        }

        public string name;
        public Dictionary<object, object> lastmsg;
        public IManagedMqttClient _mqttClient;
        public ManagedMqttClientOptions options;
        public List<object[]> outToMQTT;
    }
}
