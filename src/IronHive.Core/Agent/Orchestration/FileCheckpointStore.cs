using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IronHive.Abstractions.Agent.Orchestration;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// JSON 파일 기반 체크포인트 저장소 구현입니다.
/// 각 체크포인트를 개별 JSON 파일로 저장합니다.
/// </summary>
public class FileCheckpointStore : ICheckpointStore
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileCheckpointStore(string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        _basePath = basePath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    /// <inheritdoc />
    public async Task SaveAsync(string orchestrationId, OrchestrationCheckpoint checkpoint, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orchestrationId);
        ArgumentNullException.ThrowIfNull(checkpoint);

        Directory.CreateDirectory(_basePath);
        var filePath = GetFilePath(orchestrationId);
        var tempPath = filePath + ".tmp";

        var json = JsonSerializer.Serialize(checkpoint, _jsonOptions);
        await File.WriteAllTextAsync(tempPath, json, ct).ConfigureAwait(false);
        File.Move(tempPath, filePath, overwrite: true);
    }

    /// <inheritdoc />
    public async Task<OrchestrationCheckpoint?> LoadAsync(string orchestrationId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orchestrationId);

        var filePath = GetFilePath(orchestrationId);
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<OrchestrationCheckpoint>(json, _jsonOptions);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string orchestrationId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orchestrationId);

        var filePath = GetFilePath(orchestrationId);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string GetFilePath(string orchestrationId)
    {
        var safeName = ToSafeFileName(orchestrationId);
        return Path.Combine(_basePath, $"{safeName}.json");
    }

    /// <summary>
    /// 오케스트레이션 ID를 안전한 파일명으로 변환합니다.
    /// 영숫자, 하이픈, 밑줄만 허용하고 나머지는 SHA256 해시로 대체합니다.
    /// </summary>
    internal static string ToSafeFileName(string orchestrationId)
    {
        if (orchestrationId.Length <= 200 && orchestrationId.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.'))
            return orchestrationId;

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(orchestrationId));
        return Convert.ToHexStringLower(hash)[..32];
    }
}
