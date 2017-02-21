using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace TwitterApiCodingTest
{
    public class Program
    {
        private const string ConsumerKey = "FoX8BISvlg4lHIyF1kSrtEdsQ";
        private const string ConsumerSecret = "BHZSEeShrMs68FkhFLGhibU5Z6sx6DliEkbAVvcDVkRcYMUYDl";
        private const string AccessToken = "833932979814625280-H2l0J1hY8uAKvTw2jqRUKIN3NNp4dy3";
        private const string AccessSecret = "34VPofvFWok5J9yIrfW3I1qXrMwzxlubnQsfr4OZ4qlCF";
        private const int TweetsCount = 5;

        public static void Main(string[] args)
        {
            // settings for authentication and encoding of twitter 
            Console.OutputEncoding = Encoding.Unicode;
            Auth.SetUserCredentials(ConsumerKey, ConsumerSecret, AccessToken, AccessSecret);

            while (true)
            {
                Console.Write("Please, enter twitter username: ");
                var inputString = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(inputString))
                {
                    break;
                }

                PrintMostFrequentCharacters(inputString);
            }
        }

        private static void PrintMostFrequentCharacters(string inputString)
        {
            string username;
            if (!TryParseUsername(inputString, out username))
            {
                return;
            }

            List<ITweet> tweets;
            if (!TryGetTweets(username, out tweets))
            {
                return;
            }

            if (tweets.Count != TweetsCount)
            {
                Console.WriteLine("There are not enough tweets");
                return;
            }

            var chars = tweets.SelectMany(x => x.Text)
                .GroupBy(x => x)
                .Where(x => !string.IsNullOrWhiteSpace(x.Key.ToString()))
                .Select(x => new {Char = x.Key, Count = x.Count()})
                .ToList();
            var max = chars.Select(x => x.Count).Max();

            if (chars.Count == 0)
            {
                Console.WriteLine("All tweets are empty");
                return;
            }

            var mostFrequentChars = chars.Where(x => x.Count == max).Select(x => x.Char.ToString()).ToList();
            var resultString = FormatResult(mostFrequentChars, username, max);

            Console.WriteLine(resultString);
            try
            {
                Tweet.PublishTweet(resultString);
            }
            catch (AggregateException)
            {
                Console.WriteLine("Some problems with network at tweet publishing");
            }
        }

        private static bool TryParseUsername(string command, out string username)
        {
            var userRegex = new Regex("^@{0,1}([A-Za-z0-9_]+)$");
            var match = userRegex.Match(command);
            if (!match.Success)
            {
                Console.WriteLine("Incorrect twitter username");
                username = null;

                return false;
            }

            username = match.Groups[1].Value;

            return true;
        }

        private static bool TryGetTweets(string username, out List<ITweet> tweets)
        {
            try
            {
                tweets = Timeline.GetUserTimeline(username, new UserTimelineParameters
                {
                    MaximumNumberOfTweetsToRetrieve = TweetsCount
                }).ToList();
            }
            catch (AggregateException)
            {
                Console.WriteLine("Some problems with network");
                tweets = null;

                return false;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("User not found or blocked");
                tweets = null;

                return false;
            }
            catch (TwitterException)
            {
                Console.WriteLine("Oops, something wrong");
                tweets = null;

                return false;
            }

            return true;
        }

        private static string FormatResult(List<string> chars, string username, int occurrencesCount)
        {
            if (chars.Count == 1)
            {
                return $"Wow, '{chars.First()}' occurrences count is {occurrencesCount} in {TweetsCount} tweets of @{username}";
            }

            return $"Wow, '{string.Join("', '", chars.Take(chars.Count - 1))}' and '{chars.Last()}' occurrences count is {occurrencesCount} in {TweetsCount} tweets of @{username}";
        }
    }
}
