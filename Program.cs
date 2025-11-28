using System;
using System.Threading;
using Magent;

class Program
{
    static async Task Main(string[] args)
    {
        var mqtt = new mqttPUB("TesteMQTT");

        // 1) iniciar cliente (sem certificados)
        mqtt.startMQTTclient("192.168.13.118", 1818, "EqnFleetTester");

        // pequena pausa para estabilizar
        Thread.Sleep(1000);

        // 2) subscrever o tópico
        string topic = "agv/subsystems/actuator/load-sensor/loaded";
        await mqtt.Subscribe(topic);

        Thread.Sleep(1000);

        // 3) ler valor em loop
        while (true)
        {
            if (mqttPUB.SubscribedValues.TryGetValue(topic, out var val))
            {
                Console.WriteLine($"[{topic}] = {val}");

                bool loaded = val == "true" || val == "1";

                if (loaded)
                    Console.WriteLine("→ CARGA DETETADA NO SENSOR");
                else
                    Console.WriteLine("→ SEM CARGA");
            }
            else
            {
                Console.WriteLine("Ainda não chegou nenhuma mensagem para esse tópico.");
            }

            Thread.Sleep(1000);
        }
    }
}
