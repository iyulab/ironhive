using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ConsoleTest;

/// <summary>
/// sqlite-vec 확장 모듈 설치 도우미
/// </summary>
public static class SqliteVecInstaller
{
    private const string BaseUri = "https://github.com/asg017/sqlite-vec/releases/download";
    private const string DefaultVersion = "0.1.7-alpha.2";
    private const string MetaFileExtension = ".meta";

    /// <summary>
    /// 지정한 디렉터리에서 sqlite-vec 확장 모듈 및 메타 정보를 반환합니다.
    /// </summary>
    public static bool TryGetModule(
        string directoryPath,
        [MaybeNullWhen(false)] out string modulePath,
        [MaybeNullWhen(false)] out Dictionary<string, string> metadata)
    {
        modulePath = null;
        metadata = null;

        if (!Directory.Exists(directoryPath))
            return false;

        // 메타파일 읽기
        var metaPath = Path.Combine(directoryPath, $"vec0{MetaFileExtension}");
        metadata = ReadMetaFile(metaPath);
        if (metadata is null)
            return false;

        // 모듈 파일 경로
        var binaryExt = GetBinaryExtension();
        var filePath = Path.Combine(directoryPath, $"vec0.{binaryExt}");

        if (!File.Exists(filePath))
            return false;

        modulePath = filePath;
        // 무결성 검증
        if (!VerifyModule(modulePath, metadata))
        {
            modulePath = null;
            metadata = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 모듈 파일과 메타정보 체크섬을 비교하여 무결성을 검증합니다. 
    /// </summary>
    public static bool VerifyModule(string modulePath, Dictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("checksum", out var expected))
            return false;

        var actual = ComputeSha256(modulePath);
        return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
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

    /// <summary> 현재 프로세스의 OS별 바이너리 확장자 반환 </summary>
    private static string GetBinaryExtension()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "so";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "dylib";
        throw new PlatformNotSupportedException("지원하지 않는 OS");
    }
    
    /// <summary> 메타파일 작성 </summary>
    private static void WriteMetaFile(string modulePath, string version, Uri downloadUri)
    {
        var checksum = ComputeSha256(modulePath);

        var lines = new List<string>
        {
            $"name=sqlite-vec",
            $"version={version}",
            $"os={GetOsPlatform()}",
            $"cpu={GetCpuArch()}",
            $"file={Path.GetFileName(modulePath)}",
            $"downloadUri={downloadUri}",
            $"installedAt={DateTime.UtcNow:O}",
            $"checksum={checksum}"
        };

        var metaPath = Path.ChangeExtension(modulePath, MetaFileExtension);
        File.WriteAllLines(metaPath, lines);
    }

    /// <summary> 메타파일 읽기 </summary>
    private static Dictionary<string, string>? ReadMetaFile(string metaPath)
    {
        if (!File.Exists(metaPath))
            return null;

        return File.ReadAllLines(metaPath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
            .Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    /// <summary> SHA256 체크섬 계산 </summary>
    private static string ComputeSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        return "sha256:" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}
