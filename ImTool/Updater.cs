using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;

namespace ImTool
{
    public class Updater
    {
        private Configuration config;
        private GitHubClient github;
        public Updater(Configuration config)
        {
             
            this.config = config;
            github = new GitHubClient(new ProductHeaderValue("ImTool"));
        }

        public async Task CheckForUpdates()
        {

            IReadOnlyList<Release> releases = await github.Repository.Release.GetAll(config.GithubRepositoryOwner, config.GithubRepositoryName);
            
            foreach (Release release in releases.OrderByDescending(x => x.CreatedAt))
            {
                Console.WriteLine(JsonConvert.SerializeObject(release, Formatting.Indented));

            }
        }
    }
}