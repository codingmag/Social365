using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAdapters.Adapters
{
    using System.Web;
    using System.Web.Mvc;

    using LinqToTwitter;
    using Models;

    public static class TwitterAdapter
    {
        public static string ConsumerKey;

        public static string ConsumerSecret;

        public static async Task<List<SocialFeedItem>> SearchAsync(string query)
        {
            var auth = new ApplicationOnlyAuthorizer()
            {
                CredentialStore =
                    new InMemoryCredentialStore()
                    {
                        ConsumerKey = ConsumerKey,
                        ConsumerSecret = ConsumerSecret,
                    }
            };

            await auth.AuthorizeAsync();
            
            var twitterCtx = new TwitterContext(auth);

            var searchResults =
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                       search.Query == query
                 select search.Statuses)
                .SingleOrDefault();

            var twitterFeed = (from result in searchResults
                               select new SocialFeedItem()
                               {
                                   DateCreated = result.CreatedAt,
                                   DetailsUrl = result.User.Url,
                                   Source = FeedSource.Twitter,
                                   Text = result.Text,
                                   ThumbnailUrl = result.User.ProfileImageUrl,
                                   Username = result.User.ScreenNameResponse
                               }).ToList();

            return twitterFeed;
        }
    }
}
