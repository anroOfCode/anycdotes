using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Anycdotes.ObjectModel;

namespace Anycdotes
{
    class DataHelper
    {
        private static List<FeedItem> _defaultData;
        private static List<FeedUser> _defaultUsers;

        private static bool _isInitialized = false;

        private static int _itemCount = 300;
        private static int _userCount = 20;
        private static int _maxSentenceCount = 7;
        private static int _maxCommentCount = 3;
        private static int _maxCommentSentenceCount = 2;

        private static int _currentFeedItemCommentCount = 0;

        private static Random _random = new Random();

        public static List<FeedItem> GetDefaultData()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return _defaultData;
        }

        public static void Initialize()
        {
            InitializeStructures();
            GenerateUsers();
            _isInitialized = true;
            //GenerateItems();
        }

        private static void InitializeStructures()
        {
            _defaultUsers = new List<FeedUser>();
            _defaultData = new List<FeedItem>();
            _lorumSentences = _corpus.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void GenerateUsers()
        {
            HashSet<string> usedNamed = new HashSet<string>();
           
            while (_defaultUsers.Count < _userCount)
            {
                string newUsername = GenerateUsername();
                if (!usedNamed.Contains(newUsername))
                {
                    DateTime newJoinDate = GenerateProbableDate(false);
                    _defaultUsers.Add(new FeedUser(
                        username: newUsername, 
                        id: _defaultUsers.Count, 
                        joinDate: newJoinDate
                        )
                    ); 

                    usedNamed.Add(newUsername);
                }
            }
        }

        private static void GenerateItems()
        {
            while (_defaultData.Count < _itemCount)
            {
                FeedItem feedItem = GenerateSingleItem(false);
                _defaultData.Add(feedItem);
            }   
        }

        public static FeedItem GenerateSingleItem(bool isCurrent)
        {
            FeedUser owningUser = _defaultUsers[_random.Next(_defaultUsers.Count())];
            DateTime postedDate = GenerateProbableDate(isCurrent);
            bool didLike = _random.Next(1) == 1 ? true : false;
            string lorumText = GenerateLorum(_random.Next(1, _maxSentenceCount));
            List<FeedItemComment> comments = GenerateComments(isCurrent);

            FeedItem feedItem = new ObjectModel.FeedItem(
                id: _defaultData.Count,
                user: owningUser,
                posted: postedDate,
                text: lorumText,
                comments: comments,
                didLike: didLike);

            return feedItem;
        }

        private static List<FeedItemComment> GenerateComments(bool isCurrent)
        {
            int numberToGenerate = _random.Next(_maxCommentCount);

            List<FeedItemComment> returnList = new List<FeedItemComment>();
            for (int i = 0; i < numberToGenerate; i++)
            {
                FeedUser owningUser = _defaultUsers[_random.Next(_defaultUsers.Count())];
                DateTime postedDate = GenerateProbableCommentDate(isCurrent);
                string lorumText = GenerateLorum(_maxCommentSentenceCount);

                returnList.Add(new FeedItemComment(
                    id: ++_currentFeedItemCommentCount,
                    user: owningUser,
                    posted: postedDate,
                    text: lorumText
                    )
                );
            }

            return returnList;
        }

        private static string GenerateLorum(int sentenceCount)
        {
            StringBuilder returnBuilder = new StringBuilder();
            for (int i = 0; i < sentenceCount; i++)
            {
                returnBuilder.Append(GetRandomItem(ref _lorumSentences) + ". ");
            }

            return returnBuilder.ToString();
        }

        private static int _offsetInSeconds = 0;
        private static DateTime _startTime = DateTime.Now;

        private static DateTime GenerateProbableDate(bool isCurrent)
        {
            if (isCurrent)
            {
                return DateTime.Now;
            }
            else
            {
                _offsetInSeconds += _random.Next(5);
                return _startTime - TimeSpan.FromSeconds(_offsetInSeconds);
            }
        }

        private static DateTime GenerateProbableCommentDate(bool isCurrent)
        {
            if (isCurrent)
            {
                return DateTime.Now;
            }
            else
            {
                _offsetInSeconds += _random.Next(5);
                return _startTime - TimeSpan.FromSeconds(_offsetInSeconds);
            }
        }

        private static string GenerateUsername()
        {
            return ToUppercase(GetRandomItem(ref _adjectives)) + ToUppercase(GetRandomItem(ref _nouns));
        }

        private static string ToUppercase(string input)
        {
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        private static string GetRandomItem(ref string[] inputList)
        {
            int nextElement = _random.Next(0, inputList.Count() - 1);
            return inputList[nextElement];
        }

        private static string[] _lorumSentences;
        private const string _corpus = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras in venenatis arcu. Praesent tristique molestie nulla nec vehicula. Suspendisse volutpat, justo sed suscipit fermentum, metus magna dignissim nisl, sit amet accumsan lorem neque ut nisi. Suspendisse a euismod leo. Praesent ac magna id turpis mollis tincidunt. Suspendisse vulputate enim sed sem laoreet commodo. Aliquam quis purus vitae mi tempor vulputate porta a nulla. Ut neque ligula, commodo ac sagittis sit amet, sagittis non sapien. Cras consequat pharetra accumsan. Duis sollicitudin sodales est, sed euismod mi dictum a. Morbi faucibus rhoncus metus. Nullam porta nulla non dolor laoreet interdum. Suspendisse tempus eros vitae sem tristique aliquam. Vivamus in nibh vel libero viverra aliquet. Etiam non sem et tortor consectetur cursus. Nunc elementum ligula at augue molestie adipiscing.Donec quis erat enim. Duis sagittis faucibus quam ut eleifend. Suspendisse ac ligula nunc. Donec dapibus dui vel magna sagittis pharetra. Vestibulum auctor ultrices mi, sed pellentesque dui ultricies non. Pellentesque ultrices feugiat pulvinar. Sed metus metus, ultrices non ultrices eget, laoreet id ante. Praesent scelerisque dapibus ante eget blandit. Aenean porttitor iaculis auctor. Integer at turpis diam, nec bibendum neque. Nulla ut dolor in nunc viverra mattis vel ut dolor. Integer vel mauris nec mi egestas posuere sed rhoncus mauris. Phasellus dui justo, consectetur quis ultrices sed, ultricies in nunc. Curabitur risus turpis, luctus vel tincidunt ac, blandit et purus. Cras a ligula lorem.Donec tincidunt lectus sit amet augue facilisis eu convallis dolor malesuada. Nam molestie fermentum tortor at interdum. Praesent dictum, nisi at fringilla tincidunt, orci nisi vestibulum eros, eget sodales lectus urna eu ipsum. Nunc lobortis lobortis odio, et scelerisque est fringilla vel. Aliquam vitae sodales dolor. Nullam nec nibh et eros facilisis aliquam. Nulla in tortor elit, vel aliquet mi. Nunc felis lectus, adipiscing vitae egestas eget, ornare nec justo. Aliquam ornare euismod sapien sed consequat. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Pellentesque at odio id nunc viverra posuere. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam erat volutpat. Nulla facilisi. Nullam fringilla lectus nec leo lobortis rutrum. Ut eu arcu dolor. Fusce mauris odio, dapibus eget accumsan nec, auctor et nisl. Etiam tristique pharetra libero ut placerat. Donec vel mauris eros. Maecenas eu nibh sit amet lacus cursus dignissim at ut elit. In vitae scelerisque libero. Pellentesque aliquet suscipit mi, et sollicitudin lorem consectetur in. Cras iaculis tellus eget nisi laoreet sagittis. Fusce hendrerit diam id dolor eleifend ultricies consequat turpis hendrerit. Fusce consectetur mi et dolor pellentesque hendrerit. Cras iaculis varius diam, a sagittis massa tincidunt at. Nulla et auctor enim. Proin interdum consectetur neque, a feugiat diam molestie vel. Nam elit purus, ultrices eu ultrices nec, faucibus non dui. Proin ullamcorper viverra arcu, sed viverra nisl tristique at. Vivamus vel dolor nibh, non molestie velit. Proin cursus dapibus ipsum in posuere. Nullam viverra elementum odio, et blandit mauris tempus ac. Cras mi lorem, rhoncus ac egestas id, hendrerit vel sapien.";
        private static string[] _adjectives = { "Cold", "Dark", "Scary", "Warm", "Bright", "Noble", "Old", "Thoughtful", "Beautiful", "Kind", "Wise", "Fuzzy", "Lonely", "Happy", "Afraid", "Whimsicle" };
        private static string[] _nouns = { "Wolf", "Clock", "Mystery", "Bear", "Night", "Day", "Chair", "Desk", "Coder", "Engineer", "Teacher", "Shoes", "Pants", "Rain", "Snow", "Hail", "Sleet", "Tree", "Shrub" };
    }
}
