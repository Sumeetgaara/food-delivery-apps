using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Consumer
{
    class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var conf = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = "localhost:9092",
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe("yourTopicName");

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(cts.Token);
                            string receivedMessage = cr.Value;
                            Location obj = new Location();
                            obj  = JsonConvert.DeserializeObject<Location>(receivedMessage);
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("http://localhost:55863");
                                var content = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("lat", obj.lat),
                                    new KeyValuePair<string, string>("lng", obj.lng)
                                });
                                var result = await client.PostAsync("/Map", content);
                                string resultContent = await result.Content.ReadAsStringAsync();
                                Console.WriteLine(resultContent);
                            }
                            Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            }
        }
    }
}
