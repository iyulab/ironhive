using FluentAssertions;
using IronHive.Storages.Azure;

namespace IronHive.Tests.Storages;

/// <summary>
/// Tests to verify AzureStorageConfig validation.
/// Issue #14: Azure config validation missing for build-time safety.
/// </summary>
public class AzureStorageConfigValidationTests
{
    [Fact]
    public void Validate_ShouldThrow_WhenStorageNameIsEmpty()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "",
            AuthType = AzureStorageAuthTypes.ConnectionString,
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*StorageName*");
    }

    [Fact]
    public void Validate_ShouldThrow_WhenConnectionStringMissing_ForConnectionStringAuth()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.ConnectionString,
            ConnectionString = ""
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public void Validate_ShouldSucceed_WithValidConnectionString()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.ConnectionString,
            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldThrow_WhenAccountNameMissing_ForAccountKeyAuth()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.AccountKey,
            AccountName = "",
            AccountKey = "testkey"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AccountName*");
    }

    [Fact]
    public void Validate_ShouldThrow_WhenAccountKeyMissing_ForAccountKeyAuth()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.AccountKey,
            AccountName = "testaccount",
            AccountKey = ""
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AccountKey*");
    }

    [Fact]
    public void Validate_ShouldSucceed_WithValidAccountKey()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.AccountKey,
            AccountName = "testaccount",
            AccountKey = "testkey"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldThrow_WhenSASTokenMissing_ForSASTokenAuth()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.SASToken,
            AccountName = "testaccount",
            SASToken = ""
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*SASToken*");
    }

    [Fact]
    public void Validate_ShouldSucceed_WithValidSASToken()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.SASToken,
            AccountName = "testaccount",
            SASToken = "sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacupitfx"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldSucceed_WithValidAzureIdentity()
    {
        // Arrange
        var config = new AzureStorageConfig
        {
            StorageName = "testcontainer",
            AuthType = AzureStorageAuthTypes.AzureIdentity,
            AccountName = "testaccount"
            // TokenCredential has a default value
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }
}
