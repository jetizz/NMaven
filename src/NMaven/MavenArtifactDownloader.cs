using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NMaven.Logging;
using NMaven.Model;

namespace NMaven
{
    public class MavenArtifactDownloader : IDisposable
    {
        private readonly ITaskLogger _logger;
        private readonly HttpClient _httpClient;
        private readonly DirectoryInfo _nmvnPackageRoot;
        private readonly MavenRepository[] _repositories;

        public MavenArtifactDownloader(ITaskLogger logger, DirectoryInfo nmvnPackageRoot, params MavenRepository[] repositories)
        {
            _logger = logger;
            _repositories = repositories;
            _nmvnPackageRoot = nmvnPackageRoot;

            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<bool> DownloadArtifactAsync(MavenReference reference)
        {
            var artifactFileInfo = reference.GetArtifactFilePath(_nmvnPackageRoot);

            if (artifactFileInfo.Exists)
            {
                if (reference.Overwrite)
                {
                    // Delete local file and proceed as if it didn't exist
                    _logger.LogMessage($"Artifact {reference.ArtifactId} already downloaded on disk. Overwriting.");
                    artifactFileInfo.Delete();
                }
                else
                {
                    // File already exists, no-op
                    _logger.LogMessage($"Artifact {reference.ArtifactId} already downloaded on disk. Skipping download.");
                    return true;
                }
            }

            foreach (var repository in _repositories)
            {
                try
                {
                    var artifact = await this.DownloadArtifactAsync(reference, repository);

                    if (!artifactFileInfo.Directory.Exists)
                    {
                        artifactFileInfo.Directory.Create();
                    }

                    File.WriteAllBytes(artifactFileInfo.FullName, artifact);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{reference.ArtifactId} not found in {repository.Name} ({repository.Url}). Error: {ex.Message}.");
                }
            }

            _logger.LogError($"Artifact {reference.ArtifactId} not found.");

            return false;
        }

        private async Task<byte[]> DownloadArtifactAsync(MavenReference reference, MavenRepository repository)
        {
            var url = reference.GetRepositoryUrl(repository);
            var auth = repository.GetBasicAuthorizationHeader();

            _logger.LogMessage($"Downloading reference {reference.ArtifactId} ({reference.GroupId}) in version {reference.Version}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            if (auth != null)
            {
                _logger.LogMessage($"Using authorization: Basic {auth}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            }

            _logger.LogMessage($"Downloading artifact from: {url}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error downloading the artifact ({(int)response.StatusCode} {response.ReasonPhrase}): {responseBody}");
            }

            var content = await response.Content.ReadAsByteArrayAsync();

            return content;
        }
    }
}
