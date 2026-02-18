using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace IronHive.Core.Utilities;

/// <summary>
/// sqlite-vec 확장 모듈 설치 도우미
/// </summary>
public static class SqliteVecInstaller
{
    public const string DefaultVersion = "0.1.7-alpha.2";

    private const string BaseUri = "https://github.com/asg017/sqlite-vec/releases/download";
    private const string MetaFileExtension = ".meta";

    /// <summary>
    /// 지정한 디렉터리에서 sqlite-vec 확장 모듈의 정보를 가져옵니다.
    /// </summary>
    public static bool TryGetModule(
        string directoryPath,
        [MaybeNullWhen(false)] out SqliteVecModule module)
    {
        module = null;

        var metaPath = Path.Combine(directoryPath, $"vec0{MetaFileExtension}");
        if (!File.Exists(metaPath))
            return false;

        module = ReadMetaFile(metaPath);
        if (module is null)
            return false;

        if (!VerifyModule(module))
            return false;

        return true;
    }

    /// <summary>
    /// 클라이언트 환경에 맞는 sqlite-vec 확장 모듈을 다운로드하여 지정한 디렉터리에 설치합니다.
    /// 이미 설치되어 있으면 덮어씁니다.
    /// </summary>
    public static async Task<string> InstallAsync(
        string directoryPath,
        string version = DefaultVersion,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directoryPath);
        var tmpTarGz = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tar.gz");
        var downloadUri = BuildDownloadUri(version);

        using (var http = new HttpClient())
        using (var resp = await http.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            resp.EnsureSuccessStatusCode();
            await using var fs = File.Create(tmpTarGz);
            await resp.Content.CopyToAsync(fs, cancellationToken);
        }

        string? outPath = null;
        try
        {
            using var file = File.OpenRead(tmpTarGz);
            using var gzip = new GZipStream(file, CompressionMode.Decompress, leaveOpen: false);
            using var tar = new TarReader(gzip);

            TarEntry? entry;
            while ((entry = tar.GetNextEntry()) is not null)
            {
                if (entry.EntryType is TarEntryType.RegularFile or TarEntryType.V7RegularFile &&
                    entry.Name.StartsWith("vec0", StringComparison.OrdinalIgnoreCase))
                {
                    outPath = Path.Combine(directoryPath, entry.Name);
                    using var outStream = File.Create(outPath, 81920);
                    entry.DataStream!.CopyTo(outStream);
                    break;
                }
            }

            if (outPath is null)
                throw new InvalidDataException("다운로드한 tar.gz 파일에 vec0 확장 모듈이 포함되어 있지 않습니다.");

            // 메타파일 작성
            WriteMetaFile(outPath, version, downloadUri);

            return outPath;
        }
        finally
        {
            try { if (File.Exists(tmpTarGz)) File.Delete(tmpTarGz); }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// 모듈 파일과 메타정보 체크섬을 비교하여 무결성을 검증합니다. 
    /// </summary>
    public static bool VerifyModule(SqliteVecModule module)
    {
        var checksum = ComputeSha256(module.FilePath);
        return string.Equals(module.Checksum, checksum, StringComparison.OrdinalIgnoreCase);
    }

    #region 내부 유틸

    /// <summary> 클라이언트 환경에 맞는 다운로드 URL 생성 </summary>
    private static Uri BuildDownloadUri(string version)
    {
        var cpu = GetCpuArch();
        var os = GetOsPlatform();
        string fileName = $"sqlite-vec-{version}-loadable-{os}-{cpu}.tar.gz";
        return new Uri($"{BaseUri}/v{version}/{fileName}");
    }

    /// <summary> 현재 프로세스의 CPU 아키텍처 문자열 반환 </summary>
    private static string GetCpuArch()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "aarch64",
            _ => throw new PlatformNotSupportedException($"지원하지 않는 CPU 아키텍처: {RuntimeInformation.ProcessArchitecture}")
        };
    }

    /// <summary> 현재 프로세스의 OS 플랫폼 문자열 반환 </summary>
    private static string GetOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "macos";
        throw new PlatformNotSupportedException("지원하지 않는 OS");
    }
    
    /// <summary> 메타파일 작성 </summary>
    private static void WriteMetaFile(string modulePath, string version, Uri downloadUri)
    {
        var checksum = ComputeSha256(modulePath);

        var lines = new List<string>
        {
            $"version={version}",
            $"file_path={modulePath}",
            $"download_uri={downloadUri}",
            $"os={GetOsPlatform()}",
            $"cpu={GetCpuArch()}",
            $"checksum={checksum}",
            $"installed_at={DateTime.UtcNow:O}",
        };

        var metaPath = Path.ChangeExtension(modulePath, MetaFileExtension);
        File.WriteAllLines(metaPath, lines);
    }

    /// <summary> 메타파일 읽기 </summary>
    private static SqliteVecModule? ReadMetaFile(string metaPath)
    {
        if (!File.Exists(metaPath))
            return null;

        var dic = File.ReadAllLines(metaPath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

        if (!dic.TryGetValue("version", out var version) ||
            !dic.TryGetValue("file_path", out var filePath) ||
            !dic.TryGetValue("download_uri", out var downloadUriStr) ||
            !dic.TryGetValue("os", out var os) ||
            !dic.TryGetValue("cpu", out var cpu) ||
            !dic.TryGetValue("checksum", out var checksum) ||
            !dic.TryGetValue("installed_at", out var installedAtStr) ||
            !Uri.TryCreate(downloadUriStr, UriKind.Absolute, out var downloadUri) ||
            !DateTimeOffset.TryParse(installedAtStr, out var installedAt))
            return null;
        
        return new SqliteVecModule
        {
            FilePath = filePath,
            Version = version,
            DownloadUri = downloadUri,
            OsPlatform = os,
            CpuArch = cpu,
            Checksum = checksum,
            InstalledAt = installedAt,
        };
    }

    /// <summary> SHA256 체크섬 계산 </summary>
    private static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        return "sha256:" + Convert.ToHexStringLower(hash);
    }

    #endregion
}

/// <summary>
/// SQLite 벡터 확장 모듈 정보
/// </summary>
public sealed class SqliteVecModule
{
    public required string FilePath { get; init; }
    public required string Version { get; init; }
    public required Uri DownloadUri { get; init; }
    public required string OsPlatform { get; init; }
    public required string CpuArch { get; init; }
    public required string Checksum { get; init; }
    public required DateTimeOffset InstalledAt { get; init; }
}