using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using System;

namespace LYTest.Mis.IMICP
{
    internal class KafkaClient :IDisposable
    {
        Producer producer;
        public KafkaClient(string url)
        {

        }

        //private static readonly object locker = new object();//创建锁
        public bool SendToKafka(string topic, string message, string url)
        {
            var options = new KafkaOptions(new Uri(url));
            producer = new Producer(new BrokerRouter(options))
            {
                BatchSize = 100,
                BatchDelayTime = TimeSpan.FromMilliseconds(10)
            };
            bool ret;
            try
            {
                producer.SendMessageAsync(topic, new[] { new Message(message) });

                ret = true;
            }
            catch
            {
                ret = false;
            }
            producer.Dispose();
            return ret;
        }

        public void Dispose()
        {
            producer = null;
        }
    }

}
