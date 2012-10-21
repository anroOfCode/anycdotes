using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anycdotes.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Anycdotes
{
    class VisualFeedItem
    {
        private FeedItem _feedItem;
        private ContentPresenter _contentPresenter;
        private double _horizontalPosition = 0.0;
        private bool _isAttached = false;
        private double _verticalPosition = 0.0;
        private double _scalingFactor = 1.0;
        private bool _isVisible = false;
        private double _scaledHeight = 0.0;
        private double _unscaledHeight = 0.0;
        private bool _isFirstRealize = true;
        private int _verticalIndex;

        public VisualFeedItem(FeedItem feedItem)
        {
            _feedItem = feedItem;
        }

        #region Public Properties
        public int VerticalIndex
        {
            get
            {
                return _verticalIndex;
            }
            set
            {
                _verticalIndex = value;
            }
        }

        public FeedItem Item
        {
            get
            {
                return _feedItem;
            }
        }


        public double HorizontalPosition
        {
            get
            {
                return _horizontalPosition;
            }
            set
            {
                _horizontalPosition = value;
                if (_contentPresenter != null)
                {
                    Canvas.SetLeft(_contentPresenter, _horizontalPosition);
                }
            }
        }

        public double VerticalPosition
        {
            get
            {
                return _verticalPosition;
            }
            set
            {
                _verticalPosition = value;
                if (_contentPresenter != null)
                {
                    Canvas.SetTop(_contentPresenter, _verticalPosition);
                }
            }
        }


        public double ScalingFactor
        {
            get
            {
                return _scalingFactor;
            }
            set
            {
                _scalingFactor = value;
                if (_contentPresenter != null)
                {
                    UpdateScaledHeight();
                }
            }
        }


        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                UpdateVisibility();
            }
        }

        public double ScaledHeight { get { return _scaledHeight; } }
        public double UnscaledHeight { get { return _unscaledHeight; } }

        #endregion

        private void UpdateVisibility()
        {
            if (_contentPresenter != null)
            {
                if (_isVisible)
                {
                    Canvas.SetTop(_contentPresenter, _verticalPosition);
                }
                else
                {
                    Canvas.SetTop(_contentPresenter, -500);
                }

                if (_isFirstRealize)
                {
                    _contentPresenter.Opacity = 0.0;
                    if (_isVisible)
                    {
                        _isFirstRealize = false;
                        DoubleAnimation da = new DoubleAnimation()
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            FillBehavior = FillBehavior.HoldEnd,
                            BeginTime = TimeSpan.FromMilliseconds(_verticalIndex * 100)
                        };

                        Storyboard.SetTarget(da, _contentPresenter);
                        Storyboard.SetTargetProperty(da, "Opacity");

                        Storyboard sb = new Storyboard();
                        sb.Children.Add(da);
                        sb.Begin();
                    }
                }
                else
                {
                    _contentPresenter.Opacity = 1.0;
                }
            }
        }

        private void UpdateScaledHeight()
        {
            if (_contentPresenter != null)
            {
                double innerScaledHeight = (_unscaledHeight - _contentPresenter.Margin.Top - _contentPresenter.Margin.Bottom) * _scalingFactor;
                _contentPresenter.Height = innerScaledHeight;
                _contentPresenter.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                _scaledHeight = _contentPresenter.DesiredSize.Height;
            }
        }

        public void AttachContentPresenter(ContentPresenter cp)
        {
            if (!_isAttached)
            {
                _contentPresenter = cp;
                _contentPresenter.Content = _feedItem;
                _contentPresenter.Height = Double.NaN;
                _contentPresenter.UpdateLayout();
                _contentPresenter.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                _unscaledHeight = _contentPresenter.DesiredSize.Height;
                UpdateScaledHeight();
                UpdateVisibility();
                Canvas.SetLeft(_contentPresenter, _horizontalPosition);
                _isAttached = true;
            }
        }

        public ContentPresenter DetachContentPresenter()
        {
            if (_isAttached)
            {
                ContentPresenter cp = _contentPresenter;
                cp.Content = null;
                Canvas.SetTop(cp, -500);
                _contentPresenter = null;
                _isAttached = false;
                return cp;
            }
            else
            {
                return null;
            }
        }
    }
}