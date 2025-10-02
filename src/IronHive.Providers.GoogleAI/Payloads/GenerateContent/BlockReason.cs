namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>
/// 컨텐츠 차단 사유.
/// </summary>
internal enum BlockReason
{
    BLOCK_REASON_UNSPECIFIED,
    SAFETY,
    OTHER,
    BLOCKLIST,
    PROHIBITED_CONTENT,
    IMAGE_SAFETY
}