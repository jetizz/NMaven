using FluentAssertions;
using Moq;
using NMaven;
using NMaven.Logging;
using Tests.NMaven.Model;

namespace Tests.NMaven
{
    [TestFixture]
    public class MavenArtifactDeployerTester
    {
        private const string PackageRoot = "Packages";
        private const string DeployRoot = "Deploy";

        private static readonly DirectoryInfo PackageRootInfo = new DirectoryInfo(PackageRoot);
        private static readonly DirectoryInfo DeployRootInfo = new DirectoryInfo(DeployRoot);

        [SetUp]
        public void SetUp()
        {
            this.Cleanup();

            PackageRootInfo.Create();
            var packageFolder = PackageRootInfo
                .CreateSubdirectory("commons-compress")
                .CreateSubdirectory("1.23.0");

            File.Copy("Data\\commons-compress-1.23.0.jar", Path.Combine(packageFolder.FullName, "commons-compress-1.23.0.jar"));
        }

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(PackageRoot))
            {
                Directory.Delete(PackageRoot, true);
            }

            if (Directory.Exists(DeployRoot))
            {
                Directory.Delete(DeployRoot, true);
            }
        }

        [Test]
        public void ShouldUnzipArtifact()
        {
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");

            var logger = new Mock<ITaskLogger>();

            var deployer = new MavenArtifactDeployer(logger.Object, PackageRootInfo);

            deployer.Deploy(reference);

            var artifactDirectoryInfo = reference.GetArtifactDirectory(PackageRootInfo);

            var files = artifactDirectoryInfo.GetFiles();
            var directories = artifactDirectoryInfo.GetDirectories();

            (files.Length + directories.Length).Should().BeGreaterThan(1);
        }

        [Test]
        public void ShouldInformArtifactAlreadyUnzipped()
        {
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");

            var logger = new Mock<ITaskLogger>();

            var deployer = new MavenArtifactDeployer(logger.Object, PackageRootInfo);

            // Unzip
            deployer.Deploy(reference);

            // Already unzipped
            deployer.Deploy(reference);

            logger.Verify(l => l.LogMessage($"Artifact {reference.ArtifactId} already unzipped. Skipping unzip."), Times.Once);
        }

        [Test]
        public void ShouldExtractReferenceManifest()
        {
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");
            var deployment = ModelFactory.CreateNMavenDeployment("MANIFEST", "commons-compress", "MANIFEST.MF", DeployRoot);

            var logger = new Mock<ITaskLogger>();

            var deployer = new MavenArtifactDeployer(logger.Object, PackageRootInfo, deployment);

            deployer.Deploy(reference);

            DeployRootInfo.EnumerateFiles("MANIFEST.MF").Should().HaveCount(1);
        }

        [Test]
        public void ShouldFailExtractReferenceFakeFile()
        {
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");
            var deployment = ModelFactory.CreateNMavenDeployment("FakeFile", "commons-compress", "fake/path/to.file", DeployRoot);

            var logger = new Mock<ITaskLogger>();

            var deployer = new MavenArtifactDeployer(logger.Object, PackageRootInfo, deployment);

            deployer.Deploy(reference);

            (!DeployRootInfo.Exists || DeployRootInfo.GetFiles().Length == 0).Should().BeTrue();

            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
