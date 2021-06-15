using System;
using System.Threading.Tasks;
using Octokit;

namespace ImTool
{
    public class Updater
    {
        private GitHubClient github;
        public Updater()
        {
            this.github = new GitHubClient(new ProductHeaderValue("ImTool"));
        }

        public async Task CheckForUpdates()
        {
            var v = await github.Repository.Release.GetAll("themeldingwars", "Pyre");
            foreach (Release release in v)
            {
                Console.Write(release.TargetCommitish);
            }
            
        }
    }
}