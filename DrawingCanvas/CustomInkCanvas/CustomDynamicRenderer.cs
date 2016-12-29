using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;

namespace CustomInkCanvas
{
    class CustomDynamicRenderer : DynamicRenderer
    {
        public event OnDrawStylusPoint onDraw;
        public delegate void OnDrawStylusPoint(CustomDynamicRenderer r, StylusPointCollection s);


        protected override void OnDraw(DrawingContext drawingContext,
                                       StylusPointCollection stylusPoints,
                                       Geometry geometry, Brush fillBrush)
        {
            base.OnDraw(drawingContext, stylusPoints, geometry, fillBrush);
            onDraw?.Invoke(this, stylusPoints);
        }
    }
}
