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
