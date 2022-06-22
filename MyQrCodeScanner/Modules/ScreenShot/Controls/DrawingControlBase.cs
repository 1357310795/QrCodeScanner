using System;
using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public abstract class DrawingControlBase : FrameworkElement
    {
        private readonly VisualCollection _visualCollection;

        protected DrawingControlBase(int visualCount = 1)
        {
            _visualCollection = new VisualCollection(this);
            for (int i = 0; i < visualCount; i++)
            {
                _visualCollection.Add(new DrawingVisual());
            }
        }

        protected override int VisualChildrenCount => _visualCollection.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
            {
                throw new IndexOutOfRangeException();
            }

            return _visualCollection[index];
        }

        protected DrawingVisual GetDrawingVisual(int index = 0) => (DrawingVisual)GetVisualChild(index);
    }
}
