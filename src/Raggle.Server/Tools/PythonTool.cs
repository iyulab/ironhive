using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using Raggle.Core.ChatCompletion;
using System.ComponentModel;
using System.Text;

namespace Raggle.Server.Tools;

public class PythonTool
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    [FunctionTool]
    [Description("파이썬 스크립트를 실행합니다.")]
    public async Task<string> ExecutePythonAsync(
        [Description("실행할 Python 스크립트 내용")] string script,
        [Description("Python 버전 (예: 3.9)")] string version,
        [Description("설치할 패키지 목록")] string[] packages)
    {
        // Dockerfile 내용 생성

        var hasPackages = packages != null && packages.Length > 0
            && packages.Any(p => !string.IsNullOrWhiteSpace(p));

        var dockerfileContent = $@"
FROM python:{version}-slim
{(hasPackages ? "RUN pip install " + string.Join(" ", packages) : "")}
WORKDIR /app
COPY script.py .
CMD [""python"", ""script.py""]
";

        // 임시 디렉토리 생성
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Dockerfile 및 스크립트 파일 생성
            File.WriteAllText(Path.Combine(tempDir, "Dockerfile"), dockerfileContent);
            File.WriteAllText(Path.Combine(tempDir, "script.py"), script);

            // 임시 디렉토리를 tarball 스트림으로 변환
            using var tarStream = CreateTarballForDockerBuild(tempDir);

            // 이미지 빌드 파라미터 설정
            var buildParameters = new ImageBuildParameters
            {
                Tags = new[] { "python-runner:latest" },
                Dockerfile = "Dockerfile"
            };

            // 빌드 로그를 저장할 StringBuilder
            var buildLogs = new StringBuilder();

            var progress = new Progress<JSONMessage>(message =>
            {
                // 빌드 로그 기록 (실패 메시지나 경고 확인)
                if (!string.IsNullOrEmpty(message.Stream))
                {
                    buildLogs.Append(message.Stream);
                }
            });

            await _client.Images.BuildImageFromDockerfileAsync(
                buildParameters,
                tarStream,
                null,
                null,
                progress,
                CancellationToken.None);

            // 빌드 완료 후 이미지 존재 여부 확인
            var images = await _client.Images.ListImagesAsync(new ImagesListParameters { All = true });
            bool imageExists = images.Any(img => img.RepoTags != null && img.RepoTags.Contains("python-runner:latest"));

            if (!imageExists)
            {
                throw new Exception($"이미지 빌드 실패. 빌드 로그: {buildLogs}");
            }

            // 컨테이너 생성
            var createResponse = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "python-runner:latest",
                HostConfig = new HostConfig
                {
                    AutoRemove = false,
                    Memory = 100 * 1024 * 1024,
                    ReadonlyRootfs = true
                }
            });

            // 컨테이너 실행
            bool started = await _client.Containers.StartContainerAsync(createResponse.ID, null);
            if (!started)
            {
                throw new Exception("컨테이너 시작에 실패했습니다.");
            }

            // 컨테이너가 종료될 때까지 대기
            await _client.Containers.WaitContainerAsync(createResponse.ID);

            // 컨테이너 로그 수집 (Follow를 false로 설정)
            var logParameters = new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = false,
                Timestamps = false
            };
            using var stream = await _client.Containers.GetContainerLogsAsync(createResponse.ID, false, logParameters);
            var (stdout, stderr) = await stream.ReadOutputToEndAsync(new CancellationToken());

            // 로그 수집 후 컨테이너 삭제
            await _client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters { Force = true });

            // 결과 반환
            return stdout + stderr;
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            // 임시 디렉토리 삭제
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// 지정된 디렉토리를 tarball 스트림으로 변환합니다.
    /// </summary>
    private Stream CreateTarballForDockerBuild(string directory)
    {
        var memStream = new MemoryStream();
        // TarOutputStream을 생성할 때 underlying 스트림의 소유권은 넘기지 않음
        using (var tarOutputStream = new TarOutputStream(memStream, Encoding.UTF8))
        {
            tarOutputStream.IsStreamOwner = false;
            // 디렉토리 내부의 파일 및 폴더 추가 (현재 폴더를 루트로 설정)
            AddDirectoryFilesToTar(tarOutputStream, directory, "");
            tarOutputStream.Close();
        }
        memStream.Seek(0, SeekOrigin.Begin);
        return memStream;
    }

    /// <summary>
    /// 지정된 디렉토리의 파일과 하위 디렉토리를 TarOutputStream에 추가합니다.
    /// currentFolder는 tar 내의 상대 경로입니다.
    /// </summary>
    private void AddDirectoryFilesToTar(TarOutputStream tarOutputStream, string sourceDirectory, string currentFolder)
    {
        // 파일 추가
        foreach (var file in Directory.GetFiles(sourceDirectory))
        {
            var fileBytes = File.ReadAllBytes(file);
            var entryName = string.IsNullOrEmpty(currentFolder)
                ? Path.GetFileName(file)
                : Path.Combine(currentFolder, Path.GetFileName(file)).Replace("\\", "/");

            var tarEntry = TarEntry.CreateTarEntry(entryName);
            tarEntry.Size = fileBytes.Length;
            tarOutputStream.PutNextEntry(tarEntry);
            tarOutputStream.Write(fileBytes, 0, fileBytes.Length);
            tarOutputStream.CloseEntry();
        }

        // 하위 디렉토리 추가
        foreach (var directory in Directory.GetDirectories(sourceDirectory))
        {
            var dirName = Path.GetFileName(directory);
            var entryName = string.IsNullOrEmpty(currentFolder)
                ? dirName + "/"
                : Path.Combine(currentFolder, dirName).Replace("\\", "/") + "/";

            // 디렉토리 엔트리 생성 (크기는 0)
            var dirEntry = TarEntry.CreateTarEntry(entryName);
            dirEntry.Size = 0;
            tarOutputStream.PutNextEntry(dirEntry);
            tarOutputStream.CloseEntry();

            // 재귀적으로 디렉토리 내부의 파일 추가
            AddDirectoryFilesToTar(tarOutputStream, directory,
                string.IsNullOrEmpty(currentFolder) ? dirName : Path.Combine(currentFolder, dirName));
        }
    }
}
