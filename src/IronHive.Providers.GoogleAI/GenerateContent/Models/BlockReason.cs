namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

internal enum BlockReason
{
    BLOCK_REASON_UNSPECIFIED,
    SAFETY,
    OTHER,
    BLOCKLIST,
    PROHIBITED_CONTENT,
    IMAGE_SAFETY
}
