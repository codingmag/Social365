using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAdapters
{
    public class SocialFeedItem
    {
        public string ThumbnailUrl { get; set; }

        public string Text { get; set; }

        public string DetailsUrl { get; set; }

        public FeedSource Source { get; set; }

        public DateTime DateCreated { get; set; }

        public string Username { get; set; }
    }
}
