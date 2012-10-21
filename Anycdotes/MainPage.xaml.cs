using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Anycdotes.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Anycdotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<FeedItem> _bindingData;

        private List<ContentPresenter> _realizedItems = new List<ContentPresenter>();
        private Dictionary<FeedItem, Point> _itemPositions = new Dictionary<FeedItem, Point>();
        private double _shiftedX = 0.0;

        public MainPage()
        {
            InitializeComponent();
            this.MainCanvas.Initialize(new FeedItemSource(), Resources["ContentTemplate"] as DataTemplate, 420);
            this.Loaded += MainPage_Loaded;
            this.MainCanvas.ColumnLoaded += MainCanvas_ColumnLoaded;
        }

        void MainCanvas_ColumnLoaded(object sender, EventArgs e)
        {
            UpdateViewport();

            if (MainCanvas.RearFrontier < 0.0)
            {
                MainCanvas.ShiftColumnsRight();
                MainCanvas.Width += MainCanvas.ColumnWidth;

                if (!MainCanvas.IsEdgeLocked)
                {
                    Scroller.ScrollToHorizontalOffset(Scroller.HorizontalOffset + MainCanvas.ColumnWidth);
                }
            }
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateViewport();
        }

        private void ScrollViewer_ViewChanged_1(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateViewport();
        }

        private void UpdateViewport()
        {
            MainCanvas.ViewPort = new Rect(Scroller.HorizontalOffset, 0, Scroller.ViewportWidth, Scroller.ViewportHeight);

            // If we have less than a viewport of run room, and the frontier has shifted past
            // the current left edge of the screen, expand the canvas.
            if (MainCanvas.Width - Scroller.HorizontalOffset < Scroller.ViewportWidth * 2 &&
                MainCanvas.ForwardFrontier > Scroller.HorizontalOffset)
            {
                MainCanvas.Width += Scroller.ViewportWidth;
            }

            MainCanvas.IsEdgeLocked = (Scroller.HorizontalOffset == 0.0);

            //if (MainCanvas.RearFrontier < 0.0)
            //{
            //    MainCanvas.ShiftColumnsRight();
            //    MainCanvas.Width += MainCanvas.ColumnWidth;
            //    Scroller.ScrollToHorizontalOffset(Scroller.HorizontalOffset + MainCanvas.ColumnWidth);
            //}
        }
    }
}