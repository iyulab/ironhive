using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesMessageItem), "message")]
[JsonDerivedType(typeof(ResponsesReasoningItem), "reasoning")]
[JsonDerivedType(typeof(ResponsesFunctionCallItem), "function_call")]
[JsonDerivedType(typeof(ResponsesFunctionCallOutputItem), "function_call_output")]
[JsonDerivedType(typeof(ResponsesCustomCallItem), "custom_call")]
[JsonDerivedType(typeof(ResponsesCustomCallOutputItem), "custom_call_output")]
[JsonDerivedType(typeof(ResponsesItemReferenceItem), "item_reference")]
internal abstract class ResponsesItem
{ }

internal class ResponsesMessageItem : ResponsesItem
{ }

internal class ResponsesReasoningItem : ResponsesItem
{ }

internal class ResponsesFunctionCallItem : ResponsesItem
{ }

internal class ResponsesFunctionCallOutputItem : ResponsesItem
{ }

internal class ResponsesCustomCallItem : ResponsesItem
{ }

internal class ResponsesCustomCallOutputItem : ResponsesItem
{ }

internal class ResponsesItemReferenceItem : ResponsesItem
{ }