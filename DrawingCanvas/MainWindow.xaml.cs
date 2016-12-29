using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using CustomInkCanvas;

namespace WhiteBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        // Undone strokes collection 
        private StrokeCollection _undoneCanvasLeftCollection = new StrokeCollection();
        private StrokeCollection _undoneCanvasRightCollection = new StrokeCollection();

        // Shared Strokes
        public SynchedStrokeCollection SynchedStrokes { get; set; }

        // Routed commands 
        public static RoutedCommand UndoCommand = new RoutedCommand();
        public static RoutedCommand RedoCommand = new RoutedCommand();
        public static RoutedCommand ClearCommand = new RoutedCommand();

        #endregion

        public MainWindow()
        {
            SynchedStrokes = new SynchedStrokeCollection();
            // Set the context for the binding methods running in XAML.
            // This is what makes the binding methods find the scope to run
            DataContext = this;

            // Initialization
            InitializeComponent();
            SetCanvasAttributes();
            ConfigureBindingCommands();
            SubscribeToEvents();
        }


        #region Canvas Configuration
        protected void SetCanvasAttributes()
        {
            canvasRight.DefaultDrawingAttributes = SetAttributes(Colors.Blue);
            canvasLeft.DefaultDrawingAttributes = SetAttributes(Colors.Red);
        }

        private DrawingAttributes SetAttributes(Color color)
        {
            // Create an atribute group
            DrawingAttributes generalDrawingAttributes = new DrawingAttributes();
            generalDrawingAttributes.Color = color;
            generalDrawingAttributes.Height = 20;
            generalDrawingAttributes.Width = 20;

            return generalDrawingAttributes;
        }

        private void ConfigureBindingCommands()
        {
            UndoCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            RedoCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            ClearCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
        }

        private void SubscribeToEvents()
        {
            canvasLeft.onStylusDraw += new CustomInkCanvas.CustomInkCanvas.OnStylusPointDraw(OnStylusPointsCollectedLeft);
            canvasRight.onStylusDraw += new CustomInkCanvas.CustomInkCanvas.OnStylusPointDraw(OnStylusPointsCollectedRight);

            canvasLeft.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(OnStrokeCollected);
            canvasRight.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(OnStrokeCollected);
        }
        #endregion

        #region Actions
        // Undo the last stroke
        private void undo_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Undo count: {0}", canvasRight.Strokes.Count);

            //int strokesCount = canvasRight.Strokes.Count;
            //if (strokesCount > 0)
            //{
            //    _undoneCollection.Add(canvasRight.Strokes[strokesCount - 1]);
            //    canvasRight.Strokes.RemoveAt(strokesCount - 1);
            //    canvasLeft.Strokes.RemoveAt(strokesCount - 1);
            //}
        }

        // Redo last Undo operation
        private void redo_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Redo count: {0}", canvasLeft.Strokes.Count);
            //int strokesCount = _undoneCollection.Count;
            //if (strokesCount > 0)
            //{
            //    canvasRight.Strokes.Insert(canvasRight.Strokes.Count, _undoneCollection[strokesCount - 1]);
            //    canvasLeft.Strokes.Insert(canvasRight.Strokes.Count, _undoneCollection[strokesCount - 1]);
            //    _undoneCollection.RemoveAt(strokesCount - 1);
            //}
        }

        // Undo the last stroke
        private void clear_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear - Sender {0}", (sender as CustomInkCanvas.CustomInkCanvas).Name);
            //Save all strokes before clearing the canvas. We can then redo the clear
            StrokeCollection strokeCollection;
            strokeCollection = (sender == canvasLeft) ? _undoneCanvasLeftCollection :  
            _undoneCanvasRightCollection;
          
            foreach (var stroke in (sender as CustomInkCanvas.CustomInkCanvas).Strokes.Reverse())
            {
                strokeCollection.Add(stroke);
            }
            canvasRight.Strokes.Clear();
            canvasLeft.Strokes.Clear();
        }
        #endregion

        #region Canvas drawing
        private void OnStylusPointsCollectedLeft(CustomInkCanvas.CustomInkCanvas c, StylusPointCollection s)
        {
            Stroke stroke = new Stroke(s);
            stroke.DrawingAttributes = SetAttributes(Colors.Red);
            canvasRight.Strokes.Add(stroke);
        }

        private void OnStylusPointsCollectedRight(CustomInkCanvas.CustomInkCanvas c, StylusPointCollection s)
        {
            Stroke stroke = new Stroke(s);
            stroke.DrawingAttributes = SetAttributes(Colors.Blue);
            canvasLeft.Strokes.Add(stroke);
        }

        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            Console.WriteLine("Stroke Changed - Sender {0}", (sender as CustomInkCanvas.CustomInkCanvas).Name);
            Console.WriteLine("Stroke Changed -  {0}", e.Stroke.ToString());
        }
        #endregion
    }
}