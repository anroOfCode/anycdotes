using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Anycdotes.ObjectModel;
using Windows.Foundation;

namespace Anycdotes
{
    class CanvasColumn
    {

        private FeedItemSource _fiSource;
        private Canvas _containerCanvas;
        private ContentPresenterCache _cpCache;
        private List<VisualFeedItem> _feedItems = new List<VisualFeedItem>();
        
        private double _canvasPosition;
        private bool _isLoaded = false;
        private bool _isRealized = false;

        private TextBlock _debugTextBlock;
        private int _debugColumnIndex;
        
        public CanvasColumn(Canvas containerCanvas, FeedItemSource fiSource, ContentPresenterCache cpCache, double canvasPosition, int columnIndex)
        {
            _fiSource = fiSource;
            _cpCache = cpCache;
            _containerCanvas = containerCanvas;
            _canvasPosition = canvasPosition;
            _debugColumnIndex = columnIndex;

            IAsyncAction debugTextblockCreationTask = containerCanvas.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _debugTextBlock = new TextBlock()
                {
                    Text = "Column #" + _debugColumnIndex
                };

                _containerCanvas.Children.Add(_debugTextBlock);
                Canvas.SetLeft(_debugTextBlock, _canvasPosition);
            });
        }

        public IReadOnlyList<VisualFeedItem> FeedItems { get { return _feedItems; } }
        
        public bool IsLoaded { get { return _isLoaded; } }
        public bool IsRealized { get { return _isRealized; } }

        public double CanvasPosition 
        { 
            get 
            { 
                return _canvasPosition; 
            }
            set
            {
                _canvasPosition = value;
                foreach (VisualFeedItem feedItem in _feedItems)
                {
                    feedItem.HorizontalPosition = _canvasPosition;
                }

                if (_debugTextBlock != null)
                {
                    Canvas.SetLeft(_debugTextBlock, _canvasPosition);
                }
            }
        }

        public async Task LoadItems(Size availableSize, bool isFront)
        {
            System.Diagnostics.Debug.WriteLine("Loading {0}", _debugColumnIndex);
            double currentHeight = 0.0;

            int loopCounter = 0;
            while (true)
            {
                loopCounter += 1;
                FeedItem fi = await (isFront ? _fiSource.GetOlderItem() : _fiSource.GetNewerItem());
                ContentPresenter cp = _cpCache.Get();
                VisualFeedItem vfi = new VisualFeedItem(fi)
                {
                    VerticalIndex = _feedItems.Count
                };

                vfi.AttachContentPresenter(cp);


                if (vfi.UnscaledHeight + currentHeight > availableSize.Height)
                {
                    _cpCache.Put(cp);
                    _fiSource.ReturnItem(fi);
                    break;
                }


                vfi.HorizontalPosition = _canvasPosition;
                _feedItems.Add(vfi);
                currentHeight += vfi.UnscaledHeight;
            }

            double newSizingRatio = availableSize.Height / currentHeight;
            currentHeight = 0;

            foreach (var feedItemVisual in _feedItems)
            {
                feedItemVisual.ScalingFactor = newSizingRatio;
                feedItemVisual.VerticalPosition = currentHeight;
                feedItemVisual.IsVisible = true;

                currentHeight += feedItemVisual.ScaledHeight;
            }

            _isLoaded = true;
            _isRealized = true;
        }

        public void Realize()
        {
            if (_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Realizing {0}", _debugColumnIndex);
                foreach (var visualFeedItem in _feedItems)
                {
                    visualFeedItem.AttachContentPresenter(_cpCache.Get());
                }
                _isRealized = true;
            }
        }

        public void Recycle()
        {
            if (_isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Recycling {0}", _debugColumnIndex);
                foreach (var visualFeedItem in _feedItems)
                {
                    ContentPresenter cp = visualFeedItem.DetachContentPresenter();
                    _cpCache.Put(cp);
                }
                _isRealized = false;
            }
        }
    }
}
