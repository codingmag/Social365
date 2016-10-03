using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAdapters.Adapters
{
    using System.ServiceModel.Discovery;
    using System.Web.Mvc;
    using System.Web.UI.WebControls;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Mvc;
    using Google.Apis.Blogger.v3;
    using Google.Apis.Plus.v1;
    using Google.Apis.Services;
    using Google.Apis.YouTube.v3;
    using Models;
    public static class GoogleAdapter
    {
        public static string ApiKey;

        public static List<SocialFeedItem> SearchGooglePlus(string query)
        {
            var service = new PlusService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = ApiKey });
            var activities = service.Activities.Search(query).Execute().Items;

            var googlePlusFeed = (from result in activities
                               select new SocialFeedItem()
                               {
                                   DateCreated = result.Published.Value,
                                   DetailsUrl = result.Url,
                                   Source = FeedSource.GooglePlus,
                                   Text = result.Title,
                                   ThumbnailUrl = result.Actor.Image.Url,
                                   Username = result.Actor.DisplayName
                               }).ToList();

           
            return googlePlusFeed;
        }

        public static List<SocialFeedItem> SearchYouTube(string query)
        {
            var service = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = ApiKey });
            var videoRequest = service.Search.List("snippet");
            videoRequest.Q = query;
            videoRequest.MaxResults = 50;

            var videos = videoRequest.Execute().Items;

            var youTubeVideos = (from result in videos
                                 select new SocialFeedItem()
                                  {
                                      DateCreated = result.Snippet.PublishedAt.Value,
                                      DetailsUrl = string.Format("https://www.youtube.com/watch?v={0}", result.Id.VideoId),
                                      Source = FeedSource.YouTube,
                                      Text = result.Snippet.Title,
                                      ThumbnailUrl = result.Snippet.Thumbnails.Medium.Url,
                                      Username = result.Snippet.ChannelTitle
                                 }).ToList();

            return youTubeVideos;
        }
    }
}
