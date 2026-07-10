using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Models;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components.Pages;

public partial class Chat : IDisposable
{
    [Inject]
    private IHiveService Hive { get; set; } = null!;

    private readonly List<Message> _history = [];
    private List<MessageContent>? _streamingContent;
    private bool _isStreaming;
    private string _input = string.Empty;
    private string? _errorMessage;
    private string? _selectedKey;
    private MessageThinkingEffort _thinking = MessageThinkingEffort.None;
    private List<ModelCardList> _providers = [];
    private CancellationTokenSource? _cts;

    private bool CanSend => !_isStreaming && !string.IsNullOrWhiteSpace(_input) && _selectedKey is not null;

    protected override async Task OnInitializedAsync()
    {
        var models = await Hive.Models.ListModelsAsync();
        _providers = models.Where(p => p.Models.Any()).ToList();

        var firstProvider = _providers.Count > 0 ? _providers[0] : null;
        var firstModel = firstProvider?.Models.FirstOrDefault();
        if (firstProvider is not null && firstModel is not null)
        {
            _selectedKey = $"{firstProvider.Provider}|{firstModel.ModelId}";
        }
    }

    private async Task SendAsync()
    {
        if (!CanSend)
            return;

        var (provider, model) = ParseSelectedKey();
        var userText = _input.Trim();
        _input = string.Empty;
        _errorMessage = null;
        _history.Add(Message.User(userText));

        var workingContent = new List<MessageContent>();
        _streamingContent = workingContent;
        _isStreaming = true;
        StateHasChanged();

        var request = new MessageRequest
        {
            Provider = provider,
            Model = model,
            ThinkingEffort = _thinking,
            System = "You are a helpful assistant. Keep answers concise unless asked for detail.",
            Messages = _history,
        };

        _cts = new CancellationTokenSource();
        try
        {
            await foreach (var evt in Hive.Messages.GenerateStreamingMessageAsync(request, _cts.Token))
            {
                switch (evt)
                {
                    case StreamingContentAddedResponse added:
                        while (workingContent.Count <= added.Index)
                            workingContent.Add(new TextMessageContent());
                        workingContent[added.Index] = added.Content;
                        break;
                    case StreamingContentDeltaResponse delta when delta.Index < workingContent.Count:
                        workingContent[delta.Index].Merge(delta.Delta);
                        break;
                    case StreamingContentUpdatedResponse updated when updated.Index < workingContent.Count:
                        workingContent[updated.Index].Update(updated.Updated);
                        break;
                    case StreamingMessageDoneResponse done:
                        if (done.Message is not null)
                            _history.Add(done.Message);
                        break;
                    case StreamingMessageErrorResponse error:
                        _errorMessage = error.Message;
                        break;
                }
                StateHasChanged();
            }
        }
        catch (OperationCanceledException)
        {
            _errorMessage = "Request was canceled.";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isStreaming = false;
            _streamingContent = null;
            _cts.Dispose();
            _cts = null;
            StateHasChanged();
        }
    }

    private void Cancel() => _cts?.Cancel();

    private (string Provider, string Model) ParseSelectedKey()
    {
        var parts = _selectedKey!.Split('|', 2);
        return (parts[0], parts[1]);
    }

    private static MarkupString RenderMarkdown(string value)
        => new(Markdown.ToHtml(value ?? string.Empty));

    public void Dispose()
    {
        _cts?.Cancel();
        GC.SuppressFinalize(this);
    }
}
