using System;
using System.Threading;
using Magent;

class Program
{
    static async Task Main(string[] args)
    {
        var mqtt = new mqttPUB("TesteMQTT");

        try
        {
            // 1) iniciar cliente (sem certificados)
            mqtt.startMQTTclient("192.168.13.118", 1818, "EqnFleetTester");

            string topic = "agv/subsystems/actuator/load-sensor/loaded";
            await mqtt.Subscribe(topic);

            while (true)
            {
                if (mqttPUB.SubscribedValues.TryGetValue(topic, out var val))
                {
                    Console.WriteLine($"[{topic}] = {val}");

                    bool loaded = val == "true" || val == "1";

                    if (loaded)
                    {
                        await mqtt.DisconnectAsync();
                        Console.WriteLine("→ CARGA DETETADA NO SENSOR");
                        //return true;
                        break;
                    }
                    else
                        Console.WriteLine("→ SEM CARGA");
                        //return false;
                        break;
                }
            }
        }
        finally
        {
            await mqtt.DisconnectAsync();
        }
    }
}
