﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using System.IO;

namespace MoneroTransactionSniffer
{
    interface ITwitterPoster
    {
        bool Connected { get; }
        Task PostAsync(string message);
    }

    class TwitterPoster : ITwitterPoster
    {

        public TwitterPoster(TwitterApiCredentials twitterCredentials)
        {
            // Initialize
            var appCredentials = new TwitterCredentials()
            {
                ConsumerKey = twitterCredentials.ApiKey,
                ConsumerSecret = twitterCredentials.ApiSecret,
                AccessTokenSecret = twitterCredentials.ConsumerSecret,
                AccessToken = twitterCredentials.ConsumerKey,
                BearerToken = twitterCredentials.BearerToken,
            };
            _twitterClient = new TwitterClient(appCredentials);
            _connected = true;
        }

        public bool Connected
        {
            get
            {
                return _connected;
            }
        }

        public async Task PostAsync(string message)
        {
            try
            {
                using StreamWriter file = new("Entries.txt", append: true);
                await file.WriteLineAsync(message);
            }
            catch (Exception e)
            {
                // .. Log this later.
                Console.WriteLine(e);
            }
        }

        private readonly TwitterClient _twitterClient;
        private readonly bool _connected = false;
    }
}
