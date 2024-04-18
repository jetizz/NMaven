using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NMaven.Logging;
using NMaven.Model;

namespace NMaven
{
    public class RestoreMavenArtifacts : Task
    {
        [Required]
        public string NMavenPackageRoot { get; set; }

        [Required]
        public ITaskItem[] MavenRepositories { get; set; }

        [Required]
        public ITaskItem[] MavenReferences { get; set; }

        [Required]
        public ITaskItem[] NMavenDeployments { get; set; }

        public List<string> DeployedFiles { get; set; }

        public override bool Execute()
        {
            try
            {
                var logger = new TaskLogger(this.Log);

                var repositories = this.MavenRepositories.Select(i => new MavenRepository(i)).ToList();
                var references = this.MavenReferences.Select(i => new MavenReference(i)).ToList();
                var deployments = this.NMavenDeployments.Select(i => new NMavenDeployment(i)).ToList();

                var packageRootInfo = new DirectoryInfo(this.NMavenPackageRoot);

                var artifactDeployer = new MavenArtifactDeployer(logger, packageRootInfo, deployments.ToArray());
                using (var artifactDownloader = new MavenArtifactDownloader(logger, packageRootInfo, repositories.ToArray()))
                {
                    foreach (var reference in references)
                    {
                        if (artifactDownloader.DownloadArtifactAsync(reference).GetAwaiter().GetResult())
                        {
                            var deployedFiles = artifactDeployer.Deploy(reference);

                            if (DeployedFiles == null)
                                DeployedFiles = new List<string>();
                            DeployedFiles.AddRange(deployedFiles);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
