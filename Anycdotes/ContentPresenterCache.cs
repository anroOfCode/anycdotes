using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;

namespace Anycdotes
{
    class ContentPresenterCache
    {
        public ContentPresenterCache(Canvas canvas, DataTemplate dataTemplate)
        {
            _canvas = canvas;
            _dataTemplate = dataTemplate;

            for (int i = 0; i < 10; i++)
            {
                _cpList.Enqueue(GenerateNewPresenter());
            }
        }

        private Queue<ContentPresenter> _cpList = new Queue<ContentPresenter>();

        private DataTemplate _dataTemplate;
        public DataTemplate DataTemplate
        {
            get
            {
                return _dataTemplate;
            }

            set
            {
                _dataTemplate = value;
            }
        }

        private Canvas _canvas;
        public Canvas TargetCanvas { get { return _canvas; } }
        public int Count { get { return _cpList.Count; } }

        public ContentPresenter Get()
        {
            if (_cpList.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Generating new presenter...");
                return GenerateNewPresenter();
            }
            else
            {
                return _cpList.Dequeue();
            }
        }

        public void Put(ContentPresenter cp)
        {
            if (cp == null)
                System.Diagnostics.Debugger.Break();
            Canvas.SetTop(cp, -1000);
            cp.Content = null;
            _cpList.Enqueue(cp);
        }

        private ContentPresenter GenerateNewPresenter()
        {
            ContentPresenter cp = new ContentPresenter();
            _canvas.Children.Add(cp);
            cp.ContentTemplate = _dataTemplate;
            // put the content presenter a reasonable
            // distance away from everything
            Canvas.SetTop(cp, -1000);

            return cp;
        }
    }
}
