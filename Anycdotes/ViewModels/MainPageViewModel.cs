using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Anycdotes.ObjectModel;

namespace Anycdotes.ViewModels
{
    class MainPageViewModel
    {
        public ObservableCollection<FeedItem> CurrentItems { get; private set; }

        public MainPageViewModel()
        {

        }
    }
}
