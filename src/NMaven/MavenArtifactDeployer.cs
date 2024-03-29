using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NMaven.Logging;
using NMaven.Model;

namespace NMaven
{
    public class MavenArtifactDeployer
    {
        private readonly ITaskLogger _logger;
        private readonly NMavenDeployment[] _deployments;
        private readonly DirectoryInfo _nmvnPackageRoot;

        public MavenArtifactDeployer(ITaskLogger logger, DirectoryInfo nmvnPackageRoot, params NMavenDeployment[] deployments)
        {
            _logger = logger;
            _deployments = deployments;
            _nmvnPackageRoot = nmvnPackageRoot;
        }

        public void Deploy(MavenReference reference)
        {
            this.UnzipArtifact(reference);

            var artifactDeployments = _deployments.Where(d => d.ArtifactId == reference.ArtifactId);
            var artifactDirectory = reference.GetArtifactDirectory(_nmvnPackageRoot);

            foreach (var deployment in artifactDeployments)
            {
                Deploy(artifactDirectory, deployment);
            }
        }

        private void UnzipArtifact(MavenReference reference)
        {
            try
            {
                _logger.LogMessage($"Installing artifact {reference.ArtifactId} ({reference.GroupId}) into {_nmvnPackageRoot.FullName}");

                using (var stream = reference.GetArtifactFilePath(_nmvnPackageRoot).OpenRead())
                using (var jar = new ZipArchive(stream, ZipArchiveMode.Read, false))
                {
                    jar.ExtractToDirectory(reference.GetArtifactDirectory(_nmvnPackageRoot).FullName);
                }
            }
            catch (Exception)
            {
                _logger.LogMessage($"Artifact {reference.ArtifactId} already unzipped. Skipping unzip.");
            }
        }

        private void Deploy(DirectoryInfo packageDirectory, NMavenDeployment deployment)
        {
            try
            {
                var destinationRoot = Directory.CreateDirectory(deployment.Destination);

                _logger.LogMessage($"Deploying {deployment.Name} for artifact {deployment.ArtifactId} into {destinationRoot.FullName}.");

                // This is directory name inside the package.
                var filesSubdirectory = Path.GetDirectoryName(deployment.Files);
                // All files that should be deployed (copied)
                var files = packageDirectory.GetFiles(deployment.Files, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    DirectoryInfo destinationDirectory = destinationRoot;
                    if (deployment.PreserveFolderStructure)
                    {
                        var relativePath = file.DirectoryName.Substring(packageDirectory.FullName.Length).TrimStart('\\', '/');
                        // If 'Files' was specified using a folder name, we need to remove it from the relative path too.
                        if (deployment.RemoveRelativePath && filesSubdirectory.Length > 0)
                        {
                            relativePath = relativePath.Substring(filesSubdirectory.Length).TrimStart('\\', '/');
                        }
                        // It might still be root - so avoid CreateSubdirectory as it'll break on empty string parameter.
                        if (relativePath != string.Empty)
                        {
                            destinationDirectory = destinationRoot.CreateSubdirectory(relativePath);
                        }
                    }

                    var destinationFileName = Path.Combine(destinationDirectory.FullName, file.Name);

                    _logger.LogMessage($"- Copying {file.FullName} -> {destinationFileName}");

                    File.Copy(file.FullName, destinationFileName, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot execute {deployment.Name} deployment: ${ex.Message}.");
            }
        }
    }
}
