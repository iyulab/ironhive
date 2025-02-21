namespace Raggle.Abstractions.ChatCompletion;

public class ChatCompletionModel
{
    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the model was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the model was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the owner of the model.
    /// </summary>
    public string? Owner { get; set; }
}
