using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialFeedCollector.Job.Models
{
    public class ApplicationSettings
    {
        public string Query { get; set; }

        public Uri SiteUri { get; set; }

        public string ListName { get; set; }

        public string TwitterConsumerKey { get; set; }

        public string TwitterConsumerSecret { get; set; }

        public string GoogleAPIKey { get; set; }
    }
}
