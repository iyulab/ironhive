using System.Collections;

namespace Raggle.Abstractions.Models;

/// <summary>
/// 채팅의 히스토리를 관리하는 클래스입니다.
/// </summary>
public sealed class ChatHistory : ICollection<ChatMessage>
{
    private readonly List<ChatMessage> _messages = [];

    /// <inheritdoc />
    public int Count => _messages.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <summary>
    /// 마지막 어시스턴트 메시지를 가져옵니다.
    /// </summary>
    /// <param name="message">가져온 메시지를 반환합니다.</param>
    /// <returns>마지막 메시지가 어시스턴트 메시지일 경우 <c>true</c>, 그렇지 않으면 <c>false</c>.</returns>
    public bool TryGetLastAssistantMessage(out ChatMessage message)
    {
        return TryGetLastMessage(MessageRole.Assistant, out message);
    }

    /// <summary>
    /// 마지막 사용자 메시지를 가져옵니다.
    /// </summary>
    /// <param name="message">가져온 메시지를 반환합니다.</param>
    /// <returns>마지막 메시지가 사용자 메시지일 경우 <c>true</c>, 그렇지 않으면 <c>false</c>.</returns>
    public bool TryGetLastUserMessage(out ChatMessage message)
    {
        return TryGetLastMessage(MessageRole.User, out message);
    }

    /// <summary>
    /// 사용자 메시지를 추가합니다.
    /// </summary>
    /// <param name="content">추가할 콘텐츠 블록.</param>
    public void AddUserMessage(IContentBlock content)
    {
        AddMessage(MessageRole.User, content);
    }

    /// <summary>
    /// 여러 개의 사용자 메시지를 추가합니다.
    /// </summary>
    /// <param name="contents">추가할 콘텐츠 블록 컬렉션.</param>
    public void AddUserMessages(IEnumerable<IContentBlock> contents)
    {
        AddMessages(MessageRole.User, contents);
    }

    /// <summary>
    /// 어시스턴트 메시지를 추가합니다.
    /// </summary>
    /// <param name="content">추가할 콘텐츠 블록.</param>
    public void AddAssistantMessage(IContentBlock content)
    {
        AddMessage(MessageRole.Assistant, content);
    }

    /// <summary>
    /// 여러 개의 어시스턴트 메시지를 추가합니다.
    /// </summary>
    /// <param name="contents">추가할 콘텐츠 블록 컬렉션.</param>
    public void AddAssistantMessages(IEnumerable<IContentBlock> contents)
    {
        AddMessages(MessageRole.Assistant, contents);
    }

    /// <summary>
    /// 현재 채팅 히스토리를 복제합니다.
    /// </summary>
    /// <returns>복제된 <see cref="ChatHistory"/> 인스턴스.</returns>
    public ChatHistory Clone()
    {
        var clone = new ChatHistory();
        foreach (var message in _messages)
        {
            clone.Add(new ChatMessage
            {
                Role = message.Role,
                Contents = new List<IContentBlock>(message.Contents),
                CreatedAt = message.CreatedAt
            });
        }
        return clone;
    }

    /// <inheritdoc />
    public void Add(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _messages.Add(message);
    }

    /// <inheritdoc />
    public bool Remove(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _messages.Remove(message);
    }

    /// <inheritdoc />
    public bool Contains(ChatMessage message)
    {
        if (message == null) return false;
        return _messages.Contains(message);
    }

    /// <inheritdoc />
    public void CopyTo(ChatMessage[] array, int arrayIndex)
    {
        _messages.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _messages.Clear();
    }

    /// <inheritdoc />
    public IEnumerator<ChatMessage> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #region Private Methods

    // 마지막 메시지를 가져옵니다. 마지막 메시지가 Role에 일치하지 않으면 false입니다. 
    private bool TryGetLastMessage(MessageRole role, out ChatMessage message)
    {
        message = _messages.LastOrDefault()!;
        if (message?.Role == role)
        {
            return true;
        }
        else
        {
            message = null!;
            return false;
        }
    }

    // 메시지를 마지막에 추가합니다.
    private void AddMessage(MessageRole role, IContentBlock content)
    {
        ArgumentNullException.ThrowIfNull(content);
        AddMessages(role, [ content ]);
    }

    // 여러 메시지를 마지막에 추가합니다.
    private void AddMessages(MessageRole role, IEnumerable<IContentBlock> contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        if (TryGetLastMessage(role, out var lastMessage))
        {
            foreach (var content in contents)
            {
                lastMessage.Contents.Add(content);
            }
        }
        else
        {
            _messages.Add(new ChatMessage
            {
                Role = role,
                Contents = contents.ToList(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    #endregion
}