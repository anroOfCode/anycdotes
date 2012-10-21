using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Foundation;

namespace Anycdotes
{
    class VirtualCanvas : Canvas
    {
        private Rect? _viewPort = null;
        private bool _isInitialized = false;

        private int _pendingUpdate = 0;
        private int _pendingGeneration = 0;
        private int _updateQueued = 0;

        private FeedItemSource _fiSource;
        private double _columnWidth = 0.0;
        private ContentPresenterCache _cpCache;
        private List<CanvasColumn> _canvasColumnList;
        private bool _isEdgeLocked = true;

        public event EventHandler ColumnLoaded;

        #region Properties
        public double ColumnWidth { get { return _columnWidth; } }
        public Rect ViewPort
        {
            get
            {
                return _viewPort.HasValue ? new Rect() : _viewPort.Value;
            }

            set
            {
                _viewPort = value;
                QueueUpdate();
            }
        }

        private Size InnerSize
        {
            get
            {
                return new Size(_viewPort.Value.Width, _viewPort.Value.Height - this.Margin.Top - this.Margin.Bottom);
            }
        }

        public double ForwardFrontier
        {
            get
            {
                double canvasPosition = 0.0;
                if (_canvasColumnList.Count > 0)
                {
                    canvasPosition = _canvasColumnList[_canvasColumnList.Count - 1].CanvasPosition + _columnWidth;
                }
                return canvasPosition;
            }
        }

        public double RearFrontier
        {
            get
            {
                double canvasPosition = 0.0;
                if (_canvasColumnList.Count > 0)
                {
                    canvasPosition = _canvasColumnList[0].CanvasPosition;
                }
                return canvasPosition;
            }
        }

        public bool IsEdgeLocked
        {
            get
            {
                return _isEdgeLocked;
            }
            set
            {
                _isEdgeLocked = value;
                Debug.WriteLine("EdgeLocked: {0}", _isEdgeLocked);
            }
        }

        #endregion

        public void Initialize(FeedItemSource itemSource, DataTemplate dataTemplate, double columnWidth)
        {
            _fiSource = itemSource;
            _cpCache = new ContentPresenterCache(this, dataTemplate);
            _canvasColumnList = new List<CanvasColumn>();
            _isInitialized = true;
            _columnWidth = columnWidth;
        }

        protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
        {
            base.MeasureOverride(availableSize);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            QueueUpdate();
            return finalSize;
        }

        private void QueueUpdate()
        {
            Interlocked.Exchange(ref _updateQueued, 1);
            bool previousPendingUpdate = Interlocked.Exchange(ref _pendingUpdate, 1) == 1;
            if (!previousPendingUpdate)
            {
                Interlocked.Exchange(ref _updateQueued, 0);
                Task updateTask = UpdateRealizedItems();
            }
        }

        private async Task UpdateRealizedItems()
        {
            int previousVal;
            Debug.WriteLine("Queueing a update...");

            if (!_isInitialized || !_viewPort.HasValue)
            {
                Debug.WriteLine("Update aborted...");
                Interlocked.Exchange(ref _pendingUpdate, 0);
                return;
            }

            Debug.WriteLine("CP Cache Count: {0}", _cpCache.Count);

            // Determine if we need to realize/recycle any columns
            List<CanvasColumn> recycleList = new List<CanvasColumn>();
            List<CanvasColumn> realizeList = new List<CanvasColumn>();

            foreach (CanvasColumn column in _canvasColumnList)
            {
                DetermineColumnFate(column, recycleList, realizeList);
            }

            RecycleAll(recycleList);
            RealizeAll(realizeList);

            // Launches task to be run in the background.
            Task t = ExtendEnds();

            Debug.WriteLine("Update complete...");

            if (Interlocked.Exchange(ref _updateQueued, 0) == 1)
            {
                Debug.WriteLine("Queueing an additional update.");
                Task updateTask = UpdateRealizedItems();
            }
            else
            {
                previousVal = Interlocked.Exchange(ref _pendingUpdate, 0);
                Debug.WriteLine("Prev val: {0}", previousVal);
            }
        }

        private async Task ExtendEnds()
        {
            // Ensure only ever one instance of ExtendEnds is running.
            if (Interlocked.Exchange(ref _pendingGeneration, 1) == 1)
            {
                return;
            }

            // These two conditions are muturally exclusive. If we're extending
            // to the left or right it's going to take a while for the server
            // to respond, and we don't current support cancelling that request
            // and starting another one if the user scrolls to the other end
            // of the screen.
            Debug.WriteLine(_viewPort.Value.Right + _viewPort.Value.Width - _columnWidth);
            while (this.ForwardFrontier < _viewPort.Value.Right + _viewPort.Value.Width - _columnWidth)
            { 
                await GenerateColumn(isFront: true);
            }

            Debug.WriteLine("RearFrontier: {0}\nViewPort Edge: {1}", this.RearFrontier, _viewPort.Value.Left - _viewPort.Value.Width);
            while (this.RearFrontier > _viewPort.Value.Left - _viewPort.Value.Width)
            {
                await GenerateColumn(isFront: false);
            }

            Interlocked.Exchange(ref _pendingGeneration, 0);
        }

        private async Task GenerateColumn(bool isFront)
        {
            double newHorizontalPosition = isFront ? this.ForwardFrontier : this.RearFrontier - _columnWidth;
            CanvasColumn ccol = new CanvasColumn(this, _fiSource, _cpCache, newHorizontalPosition, _canvasColumnList.Count);

            if (isFront)
            {
                _canvasColumnList.Add(ccol);
            }
            else
            {
                _canvasColumnList.Insert(0, ccol);
            }

            await ccol.LoadItems(this.InnerSize, isFront);
            OnColumnLoaded();
        }

        #region Helpers
        private void DetermineColumnFate(CanvasColumn canvasColumn, List<CanvasColumn> recycleList, List<CanvasColumn> realizeList)
        {

            // if it's outside realized bounds
            if (IsOutsideRealizedBounds(canvasColumn.CanvasPosition))
            {
                if (canvasColumn.IsRealized)
                {
                    recycleList.Add(canvasColumn);
                }
            }
            else
            {
                if (!canvasColumn.IsRealized)
                {
                    realizeList.Add(canvasColumn);
                }
            }
        }

        private void RecycleAll(List<CanvasColumn> list)
        {
            foreach (var column in list)
            {
                column.Recycle();
            }
        }

        private void RealizeAll(List<CanvasColumn> list)
        {
            foreach (var column in list)
            {
                column.Realize();
            }
        }

        private bool IsOutsideRealizedBounds(double horizontalPosition)
        {
            return horizontalPosition < _viewPort.Value.Left - _viewPort.Value.Width ||
                horizontalPosition + _columnWidth > _viewPort.Value.Right + _viewPort.Value.Width;
        }

        private void OnColumnLoaded()
        {
            var handler = ColumnLoaded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void ShiftColumnsRight()
        {
            foreach (CanvasColumn column in _canvasColumnList)
            {
                column.CanvasPosition += _columnWidth;
            }
        }

        #endregion
    }
}
