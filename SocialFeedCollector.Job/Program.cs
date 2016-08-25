﻿using SocialMediaAdapters;
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
        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SharePointSiteUrl"]))
            {
                throw new ArgumentNullException("SharePointSiteUrl");
            }

            var siteUri = new Uri(ConfigurationManager.AppSettings["SharePointSiteUrl"]);
            
            //Get the realm for the URL
            var realm = TokenHelper.GetRealmFromTargetUrl(siteUri);

            // Get the access token for the URL.  
            //   Requires this app to be registered with the tenant
            var accessToken = TokenHelper.GetAppOnlyAccessToken(
                TokenHelper.SharePointPrincipal,
                siteUri.Authority, realm).AccessToken;

            // Get client context with access token
            using (var clientContext =
                TokenHelper.GetClientContextWithAccessToken(
                    siteUri.ToString(), accessToken))
            {
                Task.Run(async () =>
                {
                    // Do any async anything you need here without worry
                }).Wait();
            }
        }

        private static async Task<List<SocialFeedItem>> SearchTwitter()
        {
            TwitterAdapter.ConsumerKey = ConfigurationManager.AppSettings["TwitterConsumerKey"].ToString();
            TwitterAdapter.ConsumerSecret = System.Web.HttpContext.Current.Application["TwitterConsumerSecret"].ToString();
            var results = await TwitterAdapter.SearchAsync(query);
        }
    }
}
