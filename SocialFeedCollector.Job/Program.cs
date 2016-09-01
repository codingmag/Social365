using Microsoft.SharePoint.Client;
using SocialMediaAdapters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialFeedCollector.Job
{
    class Program
    {
        private const string Query = "brexit"; 

        static void Main(string[] args)
        {
            Task.Run(async () => { MainAsync(); }).Wait();
        }

        private static async Task MainAsync()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SharePointSiteUrl"]))
            {
                throw new ArgumentNullException("SharePointSiteUrl");
            }

            var siteUri = new Uri(ConfigurationManager.AppSettings["SharePointSiteUrl"]);

            var realm = TokenHelper.GetRealmFromTargetUrl(siteUri);
            var accessToken = TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, siteUri.Authority, realm).AccessToken;
            using (var clientContext = TokenHelper.GetClientContextWithAccessToken(siteUri.ToString(), accessToken))
            {
                var tweets = await SearchTwitterAsync(Query);
                foreach (var tweet in tweets)
                {
                    if (!ItemExists(clientContext))
                    {
                        AddItem(clientContext, tweet);
                    }
                }
            }
        }

        private static void AddItem(ClientContext context, SocialFeedItem tweet)
        {
            throw new NotImplementedException();
        }

        private static async Task<List<SocialFeedItem>> SearchTwitterAsync(string query)
        {
            TwitterAdapter.ConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"].ToString();
            TwitterAdapter.ConsumerSecret = System.Web.HttpContext.Current.Application["TwitterConsumerSecret"].ToString();
            var results = await TwitterAdapter.SearchAsync(query);
            return results;
        }

        private static bool ItemExists(ClientContext context)
        {
            
        }
    }
}
