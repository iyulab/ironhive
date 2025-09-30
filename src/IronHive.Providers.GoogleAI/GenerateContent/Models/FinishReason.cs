namespace IronHive.Providers.GoogleAI.GenerateContent.Models;

internal enum FinishReason
{
    FINISH_REASON_UNSPECIFIED,
    STOP,
    MAX_TOKENS,
    SAFETY,
    RECITATION, 
    LANGUAGE,
    OTHER,
    BLOCKLIST,
    PROHIBITED_CONTENT,
    SPII,
    MALFORMED_FUNCTION_CALL,
    IMAGE_SAFETY,
    UNEXPECTED_TOOL_CALL,
    TOO_MANY_TOOL_CALLS
}
