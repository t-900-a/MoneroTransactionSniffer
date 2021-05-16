using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace MoneroTransactionSniffer
{
    interface ITwitterPoster
    {
        bool Connected { get; }
        Task PostAsync(string message);
    }

    class TwitterPosterConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BearerToken { get; set; }
        public string ConsumerToken { get; set; }
        public string ConsumerTokenSecret { get; set; }
    }

    class TwitterPoster : ITwitterPoster
    {

        public TwitterPoster(TwitterPosterConfiguration twitterPosterConfiguration)
        {
            // Initialize
            var appCredentials = new TwitterCredentials()
            {
                ConsumerKey = twitterPosterConfiguration.ApiKey,
                ConsumerSecret = twitterPosterConfiguration.ApiSecret,
                AccessTokenSecret = twitterPosterConfiguration.ConsumerTokenSecret,
                AccessToken = twitterPosterConfiguration.ConsumerToken,
                BearerToken = twitterPosterConfiguration.BearerToken,
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
                Console.WriteLine($"Posted! \n{message}");
                await _twitterClient.Tweets.PublishTweetAsync(message).ConfigureAwait(false);
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
