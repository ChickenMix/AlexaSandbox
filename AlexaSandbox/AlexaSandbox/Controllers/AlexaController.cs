using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using System.Linq;

namespace AlexaSandbox.Controllers
{
    [Route("api/[controller]")]
    public class AlexaController : Controller
    {
        private IOptions<AppSettings> AppSettings { get; set; }

        public AlexaController(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings;
        }

        /// <summary>
        /// This is the entry point for the Alexa skill.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public SkillResponse HandleResponse([FromBody] SkillRequest input)
        {
            //test change to trigger PR2.
            var requestType = input.GetRequestType();

            // return a welcome message
            if (requestType == typeof(LaunchRequest))
            {
                return ResponseBuilder.Ask("Welcome to the GitHub pull request count skill.", null);
            }

            // return information from an intent
            else if (requestType == typeof(IntentRequest))
            {
                // do some intent-based stuff
                var intentRequest = input.Request as IntentRequest;

                // check the name to determine what you should do
                if (intentRequest.Intent.Name.Equals("GitHubPullRequest"))
                {
                    // get the pull requests
                    var pullrequests = CountPullRequests();

                    if (pullrequests == 0)
                        return ResponseBuilder.Tell("You have no pull requests at this time.");

                    return ResponseBuilder.Tell("There are " + pullrequests.ToString() + " pull requests waiting for you at GitHub.com.");
                }
            }

            return ResponseBuilder.Ask("I don't understand. Can you please try again?", null);
        }

        [HttpGet]
        public int CountPullRequests()
        {
            var creds = new InMemoryCredentialStore(new Credentials(AppSettings.Value.GitHubSettings.Token));
            var client = new GitHubClient(new ProductHeaderValue(AppSettings.Value.GitHubSettings.ProductName), creds);
            var pullrequests = client.PullRequest.GetAllForRepository(
                AppSettings.Value.GitHubSettings.Owner, AppSettings.Value.GitHubSettings.Repo).Result;
            return pullrequests.Count();
        }
    }
}
