using System.Windows.Controls;
using System.Windows.Input;

namespace CustomInkCanvas
{
    public class CustomInkCanvas : InkCanvas
    {
        public event OnStylusPointDraw onStylusDraw;
        public delegate void OnStylusPointDraw(CustomInkCanvas c, StylusPointCollection s);

        public CustomInkCanvas() : base()
        {
            // Use the custom dynamic renderer on the
            // custom InkCanvas.
            CustomDynamicRenderer customRenderer = new CustomDynamicRenderer();
            DynamicRenderer = customRenderer;
            customRenderer.onDraw += new CustomDynamicRenderer.OnDrawStylusPoint(StylusPointDraw);
        }

        private void StylusPointDraw(CustomDynamicRenderer c, StylusPointCollection stylusPoints)
        {
            onStylusDraw?.Invoke(this, stylusPoints);
        }
    }
}
