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
                var destination = Directory.CreateDirectory(deployment.Destination);

                _logger.LogMessage($"Deploying {deployment.Name} for artifact {deployment.ArtifactId} into {destination.FullName}.");

                var files = packageDirectory.GetFiles(deployment.Files, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    _logger.LogMessage($"- Copying {file.FullName}");

                    File.Copy(file.FullName, Path.Combine(deployment.Destination, file.Name), true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot execute {deployment.Name} deployment: ${ex.Message}.");
            }
        }
    }
}
