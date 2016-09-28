using Microsoft.SharePoint.Client;
using SocialFeedCollector.Job.Models;
using SocialMediaAdapters;
using SocialMediaAdapters.Adapters;
using SocialMediaAdapters.Models;
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
        private static ApplicationSettings appSettings;
        
        public static ApplicationSettings AppSettings
        {
            get
            {
                if (appSettings == null)
                {
                    appSettings = new ApplicationSettings();
                    GetValidateAppSettings();
                }

                return appSettings;
            }
        }

        static void Main(string[] args)
        {
            var mainTask = MainAsync();
            mainTask.Wait();
        }

        private static async Task MainAsync()
        {
            var realm = TokenHelper.GetRealmFromTargetUrl(AppSettings.SiteUri);
            var accessToken = TokenHelper.GetAppOnlyAccessToken(TokenHelper.SharePointPrincipal, AppSettings.SiteUri.Authority, realm).AccessToken;
            using (var clientContext = TokenHelper.GetClientContextWithAccessToken(AppSettings.SiteUri.ToString(), accessToken))
            {
                // Check if the list to insert tweets exists
                if (!ListExists(clientContext, AppSettings.ListName))
                {
                    throw new Exception(string.Format("The list with name {0} has not been found in the target SharePoint site {1}.", AppSettings.ListName, AppSettings.SiteUri.ToString()));
                }

                // Get tweets for the query
                var tweets = await SearchTwitterAsync(AppSettings.Query);

                // Add the tweets to SharePoint list
                foreach (var tweet in tweets)
                {
                    if (!ItemExists(clientContext, AppSettings.ListName, tweet))
                    {
                        AddItem(clientContext, AppSettings.ListName, tweet);
                    }
                }
            }
        }

        private static void GetValidateAppSettings()
        {
            // Get and set SP Site URL
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SharePointSiteUrl"]))
            {
                throw new SettingsPropertyNotFoundException("SharePointSiteUrl");
            }

            appSettings.SiteUri = new Uri(ConfigurationManager.AppSettings["SharePointSiteUrl"]);

            // Get and set SP List name
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SharePointListName"]))
            {
                throw new SettingsPropertyNotFoundException("SharePointListName");
            }

            appSettings.ListName = ConfigurationManager.AppSettings["SharePointListName"];

            // Get and set Query
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SocialMediaQuery"]))
            {
                throw new SettingsPropertyNotFoundException("SocialMediaQuery");
            }

            appSettings.Query = ConfigurationManager.AppSettings["SocialMediaQuery"];

            // Get and set Twitter Consumer Key
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["TwitterConsumerKey"]))
            {
                throw new SettingsPropertyNotFoundException("TwitterConsumerKey");
            }

            appSettings.TwitterConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"];

            // Get and set Twitter Consumer Secret
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["TwitterConsumerSecret"]))
            {
                throw new SettingsPropertyNotFoundException("TwitterConsumerSecret");
            }

            appSettings.TwitterConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"];

            // Get and set Google API Key
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["GoogleAPIKey"]))
            {
                throw new SettingsPropertyNotFoundException("GoogleAPIKey");
            }

            appSettings.GoogleAPIKey = ConfigurationManager.AppSettings["GoogleAPIKey"];


        }

        private static void AddItem(ClientContext context, string listTitle, SocialFeedItem socialFeedItem)
        {
            var list = context.Web.Lists.GetByTitle(listTitle);
            context.ExecuteQuery();
            var listItemInfo = new ListItemCreationInformation();
            var listItem = list.AddItem(listItemInfo);
            listItem["Title"] = socialFeedItem.Username;
            listItem["Text"] = socialFeedItem.Text;
            listItem["Source"] = socialFeedItem.Source.ToString();
            listItem["Thumbnail"] = new FieldUrlValue()
            {
                Url = socialFeedItem.ThumbnailUrl,
                Description = "User Image"
            };
            listItem["DateCreated"] = socialFeedItem.DateCreated;
            listItem["DetailsLink"] = new FieldUrlValue()
            {
                Url = socialFeedItem.DetailsUrl,
                Description = "User Details"
            };
            listItem.Update();
            context.ExecuteQuery();
        }

        private static async Task<List<SocialFeedItem>> SearchTwitterAsync(string query)
        {
            TwitterAdapter.ConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"].ToString();
            TwitterAdapter.ConsumerSecret = ConfigurationManager.AppSettings["TwitterConsumerSecret"].ToString();

            var results = await TwitterAdapter.SearchAsync(query);
            
            return results;
        }

        private static bool ListExists(ClientContext context, string listTitle)
        {
            var listCollection = context.Web.Lists;
            context.Load(listCollection, lists => lists.Include(list => list.Title).Where(list => list.Title == listTitle));
            context.ExecuteQuery();
            return listCollection.Count > 0;
        }

        private static bool ItemExists(ClientContext context, string listTitle, SocialFeedItem socialFeedItem)
        {
            var list = context.Web.Lists.GetByTitle(listTitle);
            var listItemCamlQuery = new CamlQuery()
            {
                ViewXml = string.Format(@"<View>
                                            <Query>
                                                <Where>
                                                    <And>
                                                        <Eq>
                                                            <FieldRef Name='Title' />
                                                            <Value Type='Text'>{0}</Value>
                                                        </Eq>
                                                        <And>
                                                            <Eq>
                                                                <FieldRef Name='Text' />
                                                                <Value Type='Text'>{1}</Value>
                                                            </Eq>
                                                            <Eq>
                                                                <FieldRef Name='Source' />
                                                                <Value Type='Choice'>{2}</Value>
                                                            </Eq>
                                                        </And>
                                                    </And>
                                                </Where>
                                            </Query>
                                        </View>",
                                        "test",
                                        "test",
                                        "Twitter")
                                        //socialFeedItem.Username,
                                        //socialFeedItem.Text,
                                        //socialFeedItem.Source)
            };
            var listItems = list.GetItems(listItemCamlQuery);
            //context.Load(listItems, items => items.Include(item => item.Id));
            context.Load(listItems);
            try
            {
                context.ExecuteQuery();
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }
    }
}
