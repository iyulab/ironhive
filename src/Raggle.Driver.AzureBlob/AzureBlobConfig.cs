using Azure.Core;
using Azure.Identity;
using System.Text.Json.Serialization;

namespace Raggle.Driver.AzureBlob;

/// <summary>
/// Represents the various authentication types that can be used to connect to Azure Blob storage.
/// </summary>
public enum AzureBlobAuthTypes
{
    /// <summary>
    /// This method uses a full connection string that includes the account name, account key, and the Blob service endpoint.
    /// Requires the <see cref="AzureBlobConfig.ConnectionString"/> field to be set.
    /// </summary>
    ConnectionString,

    /// <summary>
    /// This method uses the account name and account key for authentication without a full connection string.
    /// Requires the <see cref="AzureBlobConfig.AccountName"/> and <see cref="AzureBlobConfig.AccountKey"/> fields to be set.
    /// </summary>
    AccountKey,

    /// <summary>
    /// This Method use SAS (Shared Access Signature) token to authenticate.
    /// Requires the <see cref="AzureBlobConfig.SASToken"/> field to be set.
    /// </summary>
    SASToken,

    /// <summary>
    /// This method utilizes Azure AD (Active Directory) based authentication, including managed identities.
    /// Uses the <see cref="AzureBlobConfig.TokenCredential"/> field, which is by default set to <see cref="DefaultAzureCredential"/>.
    /// </summary>
    AzureIdentity,
}

/// <summary>
/// Configuration class for connecting to Azure Blob storage,
/// Depending on the chosen <see cref="AuthType"/>, different fields are required.
/// </summary>
public class AzureBlobConfig
{
    /// <summary>
    /// Determines which fields are required for authentication.
    /// Default is <see cref="AzureBlobAuthTypes.ConnectionString"/>.
    /// </summary>
    public AzureBlobAuthTypes AuthType { get; set; } = AzureBlobAuthTypes.ConnectionString;

    /// <summary>
    /// Required if <see cref="AuthType"/> is set to <see cref="AzureBlobAuthTypes.ConnectionString"/>.
    /// The connection string includes the account name, account key, and Blob service endpoint.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Required if <see cref="AuthType"/> is set to <see cref="AzureBlobAuthTypes.AccountKey"/>.
    /// This field is used in conjunction with the <see cref="AccountKey"/> field to authenticate using the storage account key.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Required if <see cref="AuthType"/> is set to <see cref="AzureBlobAuthTypes.AccountKey"/>.
    /// This field is used in conjunction with the <see cref="AccountName"/> field to authenticate using the storage account key.
    /// </summary>
    public string AccountKey { get; set; } = string.Empty;

    /// <summary>
    /// Required if <see cref="AuthType"/> is set to <see cref="AzureBlobAuthTypes.SASToken"/>.
    /// This token grants restricted access to the Blob storage without exposing account keys.
    /// </summary>
    public string SASToken { get; set; } = string.Empty;

    /// <summary>
    /// Required if <see cref="AuthType"/> is set to <see cref="AzureBlobAuthTypes.AzureIdentity"/>.
    /// By default, this is set to <see cref="DefaultAzureCredential"/>, which uses managed identity or other Azure AD credentials.
    /// </summary>
    [JsonIgnore]
    public TokenCredential TokenCredential { get; set; } = new DefaultAzureCredential();
}
