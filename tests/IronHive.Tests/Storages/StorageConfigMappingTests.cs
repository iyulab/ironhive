using Amazon;
using AwesomeAssertions;
using IronHive.Storages.Amazon;
using IronHive.Storages.Azure;
using IronHive.Storages.Qdrant;
using IronHive.Storages.RabbitMQ;

namespace IronHive.Tests.Storages;

/// <summary>
/// Storage configurations reach their vendor clients unasserted, which is the same gap that let a base URL
/// sit in the credential slot of the Anthropic adapter across several releases. These configurations are
/// worse placed for it than the providers: an access key and its secret, a user name and its password, a
/// host and an API key are adjacent parameters of the same type, so a swap compiles and surfaces only as an
/// authentication failure against a live service — if it surfaces at all. Azure is the extreme case, where
/// the container name reaching the wrong place does not fail but silently addresses the wrong container.
/// </summary>
public class StorageConfigMappingTests
{
    // ---- Amazon S3 -------------------------------------------------------------------------------

    private static AmazonS3Config S3() => new()
    {
        AccessKey = "AKIA-access",
        SecretAccessKey = "secret-value",
        RegionCode = "ap-northeast-2",
        BucketName = "bucket",
    };

    [Fact]
    public void S3_AccessKeyAndSecret_StayInTheirOwnSlots()
    {
        var arguments = AmazonS3FileStorage.BuildArguments(S3());

        arguments.AccessKeyId.Should().Be("AKIA-access");
        arguments.SecretAccessKey.Should().Be("secret-value");
        arguments.AccessKeyId.Should().NotBe("secret-value", "the secret must never be sent as the key id");
    }

    [Fact]
    public void S3_RegionCode_BecomesTheRegionEndpoint()
    {
        AmazonS3FileStorage.BuildArguments(S3()).ClientConfig
            .RegionEndpoint.Should().Be(RegionEndpoint.GetBySystemName("ap-northeast-2"));
    }

    [Theory]
    [InlineData("", "secret", "ap-northeast-2", "AccessKey")]
    [InlineData("key", "", "ap-northeast-2", "SecretAccessKey")]
    [InlineData("key", "secret", "", "RegionCode")]
    public void S3_MissingRequiredSetting_NamesThatSetting(string key, string secret, string region, string expected)
    {
        var config = new AmazonS3Config
        {
            AccessKey = key,
            SecretAccessKey = secret,
            RegionCode = region,
            BucketName = "bucket",
        };

        var act = () => AmazonS3FileStorage.BuildArguments(config);

        act.Should().Throw<ArgumentException>().WithMessage($"*{expected}*");
    }

    // ---- Qdrant ----------------------------------------------------------------------------------

    [Fact]
    public void Qdrant_EveryFieldReachesItsOwnSlot()
    {
        var arguments = QdrantVectorStorage.BuildArguments(new QdrantConfig
        {
            Host = "qdrant.internal",
            Port = 6335,
            Https = true,
            ApiKey = "qdrant-key",
            GrpcTimeout = TimeSpan.FromSeconds(30),
        });

        arguments.Host.Should().Be("qdrant.internal");
        arguments.Port.Should().Be(6335);
        arguments.Https.Should().BeTrue();
        arguments.ApiKey.Should().Be("qdrant-key");
        arguments.GrpcTimeout.Should().Be(TimeSpan.FromSeconds(30));
        arguments.ApiKey.Should().NotBe("qdrant.internal", "the host must never be sent as the API key");
    }

    [Fact]
    public void Qdrant_DefaultsAreCarried_NotReplacedByTheVendors()
    {
        var arguments = QdrantVectorStorage.BuildArguments(new QdrantConfig());
        var defaults = new QdrantConfig();

        arguments.Host.Should().Be(defaults.Host);
        arguments.Port.Should().Be(defaults.Port);
        arguments.GrpcTimeout.Should().Be(defaults.GrpcTimeout);
        arguments.Https.Should().BeFalse();
    }

    // ---- RabbitMQ --------------------------------------------------------------------------------

    [Fact]
    public void RabbitMQ_CredentialsAndAddress_StayInTheirOwnSlots()
    {
        var factory = RabbitMQueueStorage.CreateConnectionFactory(new RabbitMQConfig
        {
            Host = "broker.internal",
            Port = 5673,
            UserName = "svc-user",
            Password = "svc-password",
            VirtualHost = "/tenant",
            SslEnabled = true,
            QueueName = "tasks",
        });

        factory.HostName.Should().Be("broker.internal");
        factory.Port.Should().Be(5673);
        factory.UserName.Should().Be("svc-user");
        factory.Password.Should().Be("svc-password");
        factory.VirtualHost.Should().Be("/tenant");
        factory.Ssl.Enabled.Should().BeTrue();
        factory.UserName.Should().NotBe("svc-password", "the password must never be sent as the user name");
    }

    [Fact]
    public void RabbitMQ_RecoveryIsAdapterPolicy_NotLeftToTheVendorDefault()
    {
        var factory = RabbitMQueueStorage.CreateConnectionFactory(new RabbitMQConfig { QueueName = "tasks" });

        factory.AutomaticRecoveryEnabled.Should().BeTrue();
        factory.TopologyRecoveryEnabled.Should().BeTrue();
        factory.NetworkRecoveryInterval.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void RabbitMQ_QueueNameIsNotPartOfTheConnection()
    {
        var factory = RabbitMQueueStorage.CreateConnectionFactory(new RabbitMQConfig { QueueName = "tasks" });

        factory.VirtualHost.Should().NotBe("tasks", "the queue is declared on a channel, not addressed by the connection");
    }

    // ---- Azure -----------------------------------------------------------------------------------

    private static AzureStorageConfig Azure(AzureStorageAuthTypes authType) => new()
    {
        AuthType = authType,
        StorageName = "documents",
        AccountName = "myaccount",
        AccountKey = Convert.ToBase64String("account-key"u8.ToArray()),
        SASToken = "sv=2024-01-01&sig=abc",
    };

    [Fact]
    public void Azure_AccountName_BecomesTheBlobEndpoint_AndTheContainerNameStaysOutOfIt()
    {
        var uri = AzureBlobFileStorage.GetBlobStorageUri(Azure(AzureStorageAuthTypes.AccountKey));

        uri.Should().Be(new Uri("https://myaccount.blob.core.windows.net"));
        uri.Host.Should().NotContain("documents",
            "StorageName names the container, not the account — swapping them addresses the wrong storage");
    }

    [Fact]
    public void Azure_AccountName_BecomesTheFileEndpoint()
    {
        AzureFileShareStorage.GetFileStorageUri(Azure(AzureStorageAuthTypes.AccountKey))
            .Should().Be(new Uri("https://myaccount.file.core.windows.net"));
    }

    [Fact]
    public void Azure_SharedKeyCredential_TakesTheAccountNameNotTheContainerName()
    {
        var credential = AzureBlobFileStorage.GetSharedKeyCredential(Azure(AzureStorageAuthTypes.AccountKey));

        credential.AccountName.Should().Be("myaccount");
        credential.AccountName.Should().NotBe("documents");
    }

    [Fact]
    public void Azure_SasCredential_CarriesTheToken()
    {
        AzureBlobFileStorage.GetSasTokenCredential(Azure(AzureStorageAuthTypes.SASToken))
            .Signature.Should().Be("sv=2024-01-01&sig=abc");
    }

    [Fact]
    public void Azure_MissingAccountName_NamesThatSetting()
    {
        var config = Azure(AzureStorageAuthTypes.AccountKey);
        config.AccountName = string.Empty;

        var act = () => AzureBlobFileStorage.GetBlobStorageUri(config);

        act.Should().Throw<ArgumentException>().WithMessage("*AccountName*");
    }
}
