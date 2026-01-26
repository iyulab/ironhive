using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CharCitation), "char_citation")]
[JsonDerivedType(typeof(PageCitation), "page_location")]
[JsonDerivedType(typeof(BlockCitation), "content_block_location")]
[JsonDerivedType(typeof(WebSearchCitation), "web_search_result_location")]
[JsonDerivedType(typeof(SearchCitation), "search_result_location")]
internal abstract class TextCitation
{ }

internal sealed class CharCitation : TextCitation
{
    [JsonPropertyName("cited_text")]
    public string? CitedText { get; set; }

    [JsonPropertyName("document_index")]
    public int? DocumentIndex { get; set; }

    [JsonPropertyName("document_title")]
    public string? DocumentTitle { get; set; }

    [JsonPropertyName("end_char_index")]
    public int? EndIndex { get; set; }

    [JsonPropertyName("start_char_index")]
    public int? StartIndex { get; set; }
}

internal sealed class PageCitation : TextCitation
{
    [JsonPropertyName("cited_text")]
    public string? CitedText { get; set; }

    [JsonPropertyName("document_index")]
    public int? DocumentIndex { get; set; }

    [JsonPropertyName("document_title")]
    public string? DocumentTitle { get; set; }

    [JsonPropertyName("end_page_index")]
    public int? EndIndex { get; set; }

    [JsonPropertyName("start_page_index")]
    public int? StartIndex { get; set; }
}

internal sealed class BlockCitation : TextCitation
{
    [JsonPropertyName("cited_text")]
    public string? CitedText { get; set; }

    [JsonPropertyName("document_index")]
    public int? DocumentIndex { get; set; }

    [JsonPropertyName("document_title")]
    public string? DocumentTitle { get; set; }

    [JsonPropertyName("end_block_index")]
    public int? EndIndex { get; set; }

    [JsonPropertyName("start_block_index")]
    public int? StartIndex { get; set; }
}

internal sealed class WebSearchCitation : TextCitation
{
    [JsonPropertyName("cited_text")]
    public string? CitedText { get; set; }

    [JsonPropertyName("encrypted_index")]
    public int? EncryptedIndex { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

internal sealed class SearchCitation : TextCitation
{
    [JsonPropertyName("cited_text")]
    public string? CitedText { get; set; }

    [JsonPropertyName("end_block_index")]
    public int? EndBlockIndex { get; set; }

    [JsonPropertyName("search_result_index")]
    public int? SearchResultIndex { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("start_block_index")]
    public int? StartBlockIndex { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
