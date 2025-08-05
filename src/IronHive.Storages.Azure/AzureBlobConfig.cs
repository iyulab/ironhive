using Azure.Core;
using Azure.Identity;

using System.Text.Json.Serialization;

/// <summary>
/// Azure Blob Storage에 연결하기 위해 사용할 수 있는 다양한 인증 유형을 나타냅니다.
/// </summary>
public enum AzureBlobAuthTypes
{
    /// <summary>
    /// 전체 연결 문자열을 사용하는 방식입니다. 
    /// 연결 문자열에는 계정 이름, 계정 키, Blob 서비스 엔드포인트가 포함되어 있어야 합니다.
    /// <see cref="AzureBlobConfig.ConnectionString"/> 필드가 필요합니다.
    /// </summary>
    ConnectionString,

    /// <summary>
    /// 계정 이름과 계정 키를 사용하여 인증하는 방식입니다. 연결 문자열 전체는 필요하지 않습니다.
    /// <see cref="AzureBlobConfig.AccountName"/> 및 <see cref="AzureBlobConfig.AccountKey"/> 필드가 필요합니다.
    /// </summary>
    AccountKey,

    /// <summary>
    /// SAS(공유 액세스 서명) 토큰을 사용하여 인증하는 방식입니다.
    /// <see cref="AzureBlobConfig.SASToken"/> 필드가 필요합니다.
    /// </summary>
    SASToken,

    /// <summary>
    /// Azure AD(Active Directory)를 기반으로 하는 인증 방식입니다. 관리 ID 등도 포함됩니다.
    /// <see cref="AzureBlobConfig.TokenCredential"/> 필드를 사용하며, 기본값은 <see cref="DefaultAzureCredential"/>입니다.
    /// </summary>
    AzureIdentity,
}

/// <summary>
/// Azure Blob Storage에 연결하기 위한 설정 클래스입니다.
/// 선택한 <see cref="AuthType"/>에 따라 필수로 설정해야 하는 필드가 달라집니다.
/// </summary>
public class AzureBlobConfig
{
    /// <summary>
    /// 인증 방식 설정입니다.
    /// 기본값은 <see cref="AzureBlobAuthTypes.ConnectionString"/>입니다.
    /// </summary>
    public AzureBlobAuthTypes AuthType { get; set; } = AzureBlobAuthTypes.ConnectionString;

    /// <summary>
    /// <see cref="AuthType"/>이 <see cref="AzureBlobAuthTypes.ConnectionString"/>일 경우 필수입니다.
    /// 계정 이름, 계정 키, Blob 서비스 엔드포인트가 포함된 전체 연결 문자열입니다.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="AuthType"/>이 <see cref="AzureBlobAuthTypes.AccountKey"/>일 경우 필수입니다.
    /// 계정 키와 함께 사용되어 저장소 계정 인증에 사용됩니다.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="AuthType"/>이 <see cref="AzureBlobAuthTypes.AccountKey"/>일 경우 필수입니다.
    /// 계정 이름과 함께 사용되어 저장소 계정 인증에 사용됩니다.
    /// </summary>
    public string AccountKey { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="AuthType"/>이 <see cref="AzureBlobAuthTypes.SASToken"/>일 경우 필수입니다.
    /// 계정 키를 노출하지 않고 Blob 저장소에 제한된 액세스를 부여하는 토큰입니다.
    /// </summary>
    public string SASToken { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="AuthType"/>이 <see cref="AzureBlobAuthTypes.AzureIdentity"/>일 경우 필수입니다.
    /// 기본값은 <see cref="DefaultAzureCredential"/>이며, 관리 ID 또는 다른 Azure AD 자격 증명을 사용합니다.
    /// </summary>
    [JsonIgnore]
    public TokenCredential TokenCredential { get; set; } = new DefaultAzureCredential();

    /// <summary>
    /// Azure Blob Storage에서 사용할 컨테이너 이름입니다.
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;
}
