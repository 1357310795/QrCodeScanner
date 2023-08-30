using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QRCodeMagic.Controls;

public partial class MultiColumnFlowPanel : Panel
{
    private int _visibleItemsCount = 0;
    private double colWidth = 0;

    List<List<UIElement>> itemArrs;
    List<double> itemHeights;
    public double ColSpacing
    {
        get { return (double)GetValue(ColSpacingProperty); }
        set { SetValue(ColSpacingProperty, value); }
    }
    public static readonly DependencyProperty ColSpacingProperty = DependencyProperty.Register(
        nameof(ColSpacing),
        typeof(double),
        typeof(EqualPanel),
        new PropertyMetadata(default(double), OnColSpacingChanged));

    public double ItemSpacing
    {
        get { return (double)GetValue(ItemSpacingProperty); }
        set { SetValue(ItemSpacingProperty, value); }
    }
    public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register(
        nameof(ItemSpacing),
        typeof(double),
        typeof(EqualPanel),
        new PropertyMetadata(default(double), OnItemSpacingChanged));

    public int Cols
    {
        get { return (int)GetValue(ColsProperty); }
        set { SetValue(ColsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Cols.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ColsProperty =
        DependencyProperty.Register("Cols", typeof(int), typeof(MultiColumnFlowPanel), new PropertyMetadata(1, OnColsChanged));

    public MultiColumnFlowPanel()
    {
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var elements = Children.OfType<UIElement>().Where(static e => e.Visibility == Visibility.Visible);
        _visibleItemsCount = elements.Count();

        colWidth = (availableSize.Width - ColSpacing * (Cols - 1)) / (double)(Cols);
        itemArrs = new List<List<UIElement>>();
        for (int i = 0; i < Cols; i++)
            itemArrs.Add(new List<UIElement>());
        itemHeights = new List<double>();
        for (int i = 0; i < Cols; i++)
            itemHeights.Add(0);

        foreach (var child in elements)
        {
            child.Measure(new Size(colWidth, double.PositiveInfinity));
            int minIndex = itemHeights.IndexOf(itemHeights.Min());
            itemArrs[minIndex].Add(child);
            itemHeights[minIndex] += child.DesiredSize.Height;
        }

        if (_visibleItemsCount > 0)
        {
            return new Size(availableSize.Width, itemHeights.Max());
        }
        else
        {
            return new Size(0, 0);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;

        // Check if there's more width available - if so, recalculate (e.g. whenever Grid.Column is set to Auto)
        //if (finalSize.Width > _visibleItemsCount * _maxItemWidth + Spacing * (_visibleItemsCount - 1))
        //{
        //    MeasureOverride(finalSize);
        //}

        //var elements = Children.OfType<UIElement>().Where(static e => e.Visibility == Visibility.Visible);

        for (int i = 0; i < Cols; i++)
        {
            double sum = 0;
            foreach (var child in itemArrs[i])
            {
                child.Arrange(new Rect(i*(colWidth +ColSpacing), sum, colWidth, child.DesiredSize.Height));
                sum += child.DesiredSize.Height + ItemSpacing;
            }
        }
            
        return finalSize;
    }

    private static void OnColSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (MultiColumnFlowPanel)d;
        panel.InvalidateMeasure();
    }

    private static void OnItemSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (MultiColumnFlowPanel)d;
        panel.InvalidateMeasure();
    }

    private static void OnColsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (MultiColumnFlowPanel)d;
        panel.InvalidateMeasure();
    }
}
