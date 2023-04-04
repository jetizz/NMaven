using FluentAssertions;

namespace Tests.NMaven.Model
{
    [TestFixture]
    public class MavenReferenceTester
    {
        [Test]
        public void ShouldGetDependencyUrl()
        {
            var repository = ModelFactory.CreateMavenRepository("Repo", "http://monrepo.fr");

            var dependency = ModelFactory.CreateMavenReference("artifact-id", "mon.group", "1.0.0");
            
            dependency.GetRepositoryUrl(repository)
                .Should().Be("http://monrepo.fr/mon/group/artifact-id/1.0.0/artifact-id-1.0.0.jar");
        }
    }
}
