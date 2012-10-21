using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anycdotes.ObjectModel
{
    public class FeedItem
    {
        public int Id { get; private set; }
        public FeedUser User { get; private set; }
        public string Text { get; private set; }
        public List<FeedItemComment> Comments { get; set; }
        public bool DidLike { get; private set; }
        public DateTime Posted { get; private set; }

        public FeedItem(int id, FeedUser user, DateTime posted, string text, List<FeedItemComment> comments, bool didLike)
        {
            this.Id = id;
            this.User = user;
            this.Posted = posted;
            this.Text = text;
            this.Comments = comments;
            this.DidLike = didLike;
        }
    }

    public class FeedUser
    {
        public string Username { get; private set; }
        public int Id { get; private set; }
        public DateTime JoinDate { get; private set; }

        public FeedUser(string username, int id, DateTime joinDate)
        {
            this.Username = username;
            this.Id = id;
            this.JoinDate = joinDate;
        }
    }

    public class FeedItemComment
    {
        public int Id { get; private set; }
        public FeedUser User { get; private set; }
        public DateTime Posted { get; private set; }
        public string Text { get; private set; }

        public FeedItemComment(int id, FeedUser user, DateTime posted, string text)
        {
            this.Id = id;
            this.User = user;
            this.Posted = posted;
            this.Text = text;
        }
    }
}
