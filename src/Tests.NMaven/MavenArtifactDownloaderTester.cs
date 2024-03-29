using FluentAssertions;
using Moq;
using NMaven;
using NMaven.Logging;
using Tests.NMaven.Model;

namespace Tests.NMaven
{
    [TestFixture]
    public class MavenArtifactDownloaderTester
    {
        private const string PackageRoot = "Packages";

        private static readonly DirectoryInfo PackageRootInfo = new DirectoryInfo(PackageRoot);

        [SetUp, TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(PackageRoot))
            {
                Directory.Delete(PackageRoot, true);
            }
        }

        [Test]
        public async Task ShouldDownloadArtifacts()
        {
            var repository = ModelFactory.CreateMavenRepository("Maven-Repo", "https://repo1.maven.org/maven2");
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");

            var logger = new Mock<ITaskLogger>();

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeTrue();

            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);

            PackageRootInfo.Exists.Should().BeTrue();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).Should().NotBeEmpty();
        }

        [Test]
        public async Task ShouldDetectDownloadedArtifacts()
        {
            var repository = ModelFactory.CreateMavenRepository("Maven-Repo", "https://repo1.maven.org/maven2");
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");

            var logger = new Mock<ITaskLogger>();

            PackageRootInfo.Create();
            var packageFolder = PackageRootInfo
                .CreateSubdirectory(reference.ArtifactId)
                .CreateSubdirectory(reference.Version);

            File.Copy("Data\\commons-compress-1.23.0.jar", Path.Combine(packageFolder.FullName, "commons-compress-1.23.0.jar"));

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeTrue();

            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
            logger.Verify(l => l.LogMessage(It.IsAny<string>()), Times.Once);

            PackageRootInfo.Exists.Should().BeTrue();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).Should().NotBeEmpty();
        }

        [Test]
        public async Task ShouldRedownloadArtifactsWhenOverwrite()
        {
            var repository = ModelFactory.CreateMavenRepository("Maven-Repo", "https://repo1.maven.org/maven2");
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0", overwrite: true);

            var logger = new Mock<ITaskLogger>();

            PackageRootInfo.Create();
            var packageFolder = PackageRootInfo
                .CreateSubdirectory(reference.ArtifactId)
                .CreateSubdirectory(reference.Version);

            // Create dummy file (but correct name). We'll assert that file size is not == 1 after download.
            File.WriteAllBytes(Path.Combine(packageFolder.FullName, "commons-compress-1.23.0.jar"), new byte[1] { 42 });

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeTrue();

            PackageRootInfo.Exists.Should().BeTrue();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).Should().NotBeEmpty();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).First().Length.Should().NotBe(1);
        }

        [Test]
        public async Task ShouldNotRedownloadArtifactsWhenNotOverwrite()
        {
            var repository = ModelFactory.CreateMavenRepository("Maven-Repo", "https://repo1.maven.org/maven2");
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0", overwrite: false);

            var logger = new Mock<ITaskLogger>();

            PackageRootInfo.Create();
            var packageFolder = PackageRootInfo
                .CreateSubdirectory(reference.ArtifactId)
                .CreateSubdirectory(reference.Version);

            // Create dummy file (but correct name). We'll assert that file size is not == 1 after download.
            File.WriteAllBytes(Path.Combine(packageFolder.FullName, "commons-compress-1.23.0.jar"), new byte[1] { 42 });

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeTrue();

            PackageRootInfo.Exists.Should().BeTrue();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).Should().NotBeEmpty();
            PackageRootInfo.GetFiles(reference.ArtifactFileName, SearchOption.AllDirectories).First().Length.Should().Be(1);
        }

        [Test]
        public async Task ShouldFailDownloadFakeArtifacts()
        {
            var repository = ModelFactory.CreateMavenRepository("Maven-Repo", "https://repo1.maven.org/maven2");
            var reference = ModelFactory.CreateMavenReference("fake-artifact", "fake.group", "1.3.3.7");

            var logger = new Mock<ITaskLogger>();

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeFalse();

            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ShouldFailDownloadArtifactsFromFakeRepo()
        {
            var repository = ModelFactory.CreateMavenRepository("Fake-Repo", "https://fake.repo.org/maven2");
            var reference = ModelFactory.CreateMavenReference("commons-compress", "org.apache.commons", "1.23.0");

            var logger = new Mock<ITaskLogger>();

            using var downloader = new MavenArtifactDownloader(logger.Object, PackageRootInfo, repository);

            (await downloader.DownloadArtifactAsync(reference)).Should().BeFalse();

            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
            logger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
