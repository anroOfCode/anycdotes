using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Anycdotes.ObjectModel;

namespace Anycdotes
{
    class FeedItemSource
    {
        public FeedItemSource()
        {
           DataHelper.Initialize();
        }

        private List<FeedItem> _feedItems;
        private int _currentPosition = 0;

        public async Task<FeedItem> GetOlderItem()
        {
            await Task.Delay(50);
            return DataHelper.GenerateSingleItem(false);
        }

        public async Task<FeedItem> GetNewerItem()
        {
            await Task.Delay(500);
            return DataHelper.GenerateSingleItem(true);
        }

        public void ReturnItem(FeedItem item)
        {
            // TODO: Real caching implementation. THIS IS A HACK
            //_currentPosition--;
        }

    }
}
