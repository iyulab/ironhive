using System.Text;

namespace IronHive.Storages.RabbitMQ;

public class RabbitConfig
{
    /// <summary>
    /// RabbitMQ hostname, e.g. "127.0.0.1"
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// TCP port for the connection, e.g. 5672
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Authentication username
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Authentication password
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// Queue name, e.g. "my-queue"
    /// </summary>
    public string QueueName { get; set; } = "ironhive";

    /// <summary>
    /// RabbitMQ virtual host name, e.g. "/"
    /// See https://www.rabbitmq.com/docs/vhosts
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// How long to retry messages delivery, ie how long to retry, in seconds.
    /// Default: 3600 second, 1 hour.
    /// </summary>
    public int MessageTTLSecs { get; set; } = 3600;

    /// <summary>
    /// Set to true if your RabbitMQ supports SSL.
    /// Default: false
    /// </summary>
    public bool SslEnabled { get; set; } = false;

    /// <summary>
    /// How many messages to process asynchronously at a time, in each queue.
    /// Note that this applies to each queue, and each queue is used
    /// for a specific pipeline step.
    /// </summary>
    public ushort ConcurrentThreads { get; set; } = 2;

    /// <summary>
    /// How many messages to fetch at a time from each queue.
    /// The value should be higher than ConcurrentThreads to make sure each
    /// thread has some work to do.
    /// Note that this applies to each queue, and each queue is used
    /// for a specific pipeline step.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 3;

    /// <summary>
    /// How many times to retry processing a message before moving it to a poison queue.
    /// Example: a value of 20 means that a message will be processed up to 21 times.
    /// Note: this value cannot be changed after queues have been created. In such case
    ///       you might need to drain all queues, delete them, and restart the ingestion service(s).
    /// </summary>
    public int MaxRetriesBeforePoisonQueue { get; set; } = 20;

    /// <summary>
    /// How long to wait before putting a message back to the queue in case of failure.
    /// Note: currently a basic strategy not based on RabbitMQ exchanges, potentially
    /// affecting the pipeline concurrency performance: consumers hold
    /// messages for N msecs, slowing down the delivery of other messages.
    /// </summary>
    public int DelayBeforeRetryingMsecs { get; set; } = 500;

    /// <summary>
    /// Suffix used for the poison queues.
    /// </summary>
    public string PoisonQueueSuffix { get; set; } = "-poison";
}
