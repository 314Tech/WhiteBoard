using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

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
        private Stack<Stroke> _undoLeftStrokesStack = new Stack<Stroke>();
        private Stack<Stroke> _undoRightStrokesStack = new Stack<Stroke>();

        // Shared Strokes
        public SynchedStrokeCollection SynchedStrokes { get; set; }

        // Routed commands 
        public static RoutedCommand UndoCommand = new RoutedCommand();
        public static RoutedCommand RedoCommand = new RoutedCommand();
        public static RoutedCommand ClearCommand = new RoutedCommand();

        // Temporary strokes collection
        private StrokeCollection tempStrokeCollection;

        // GUID
        private Guid metadataGuid = Guid.NewGuid();
        #endregion

        public MainWindow()
        {
            SynchedStrokes = new SynchedStrokeCollection();
            // Set the context for the binding methods running in XAML.
            // This is what makes the binding methods find the scope to run
            DataContext = this;
            tempStrokeCollection = new StrokeCollection();

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
            CustomInkCanvas.CustomInkCanvas canvas = sender as CustomInkCanvas.CustomInkCanvas;
            Console.WriteLine("Undo count: {0}", canvas.Strokes.Count);

            int strokesDeleteIndex = canvas.Strokes.Count;
            string tag = (canvas == canvasRight) ? "Blue" : "Red";
            while (strokesDeleteIndex > 0)
            {
                if (canvas.Strokes[strokesDeleteIndex - 1].GetPropertyData(metadataGuid) as string == tag)
                {
                    if (tag == "Blue")
                    {
                        _undoRightStrokesStack.Push(canvasLeft.Strokes[strokesDeleteIndex - 1]);
                    } else {
                        _undoLeftStrokesStack.Push(canvasLeft.Strokes[strokesDeleteIndex - 1]);
                    }
                    // Remove the element
                    canvasRight.Strokes.RemoveAt(strokesDeleteIndex - 1);
                    canvasLeft.Strokes.RemoveAt(strokesDeleteIndex - 1);
                    break;
                }
                strokesDeleteIndex--;
            }
        }

        // Redo last Undo operation
        private void redo_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Redo count: {0}", canvasLeft.Strokes.Count);
            CustomInkCanvas.CustomInkCanvas canvas = sender as CustomInkCanvas.CustomInkCanvas;

            Stack<Stroke> undoneStrokes = (canvas == canvasLeft) ? _undoLeftStrokesStack : _undoRightStrokesStack;
            int strokesCount = undoneStrokes.Count;
            if (strokesCount > 0)
            {
                var stroke = undoneStrokes.Pop();
                canvasRight.Strokes.Insert(canvasRight.Strokes.Count, stroke);
                canvasLeft.Strokes.Insert(canvasLeft.Strokes.Count, stroke);
            }
        }

        // Undo the last stroke
        private void clear_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear - Sender {0}", (sender as CustomInkCanvas.CustomInkCanvas).Name);
            //Save all strokes before clearing the canvas. We can then redo the clear
            Stack<Stroke> strokeCollection;
            strokeCollection = (sender == canvasLeft) ? _undoLeftStrokesStack :  
            _undoRightStrokesStack;
          
            foreach (var stroke in (sender as CustomInkCanvas.CustomInkCanvas).Strokes.Reverse())
            {
                strokeCollection.Push(stroke);
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
            tempStrokeCollection.Add(stroke);
        }

        private void OnStylusPointsCollectedRight(CustomInkCanvas.CustomInkCanvas c, StylusPointCollection s)
        {
            Stroke stroke = new Stroke(s);
            stroke.DrawingAttributes = SetAttributes(Colors.Blue);
            canvasLeft.Strokes.Add(stroke);
            tempStrokeCollection.Add(stroke);
        }

        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            Console.WriteLine("Stroke Changed - Sender {0}", (sender as CustomInkCanvas.CustomInkCanvas).Name);
            Console.WriteLine("Stroke Changed -  {0}", e.Stroke.ToString());

            Console.WriteLine("Before: Left Canvas Count -  {0}", canvasLeft.Strokes.Count);
            Console.WriteLine("Before: Right Canvas Count -  {0}", canvasRight.Strokes.Count);

            CustomInkCanvas.CustomInkCanvas sourceCanvas;
            CustomInkCanvas.CustomInkCanvas targetCanvas;

            sourceCanvas = sender as CustomInkCanvas.CustomInkCanvas;
            targetCanvas = (sender == canvasLeft) ? canvasRight : canvasLeft;

            string tag = (sender != canvasLeft) ? "Blue" : "Red";

            // Tag the stroke to show its canvas source editor
            e.Stroke.AddPropertyData(metadataGuid, tag);

            // Add last Stroke from the source canvas to the target
            StrokeCollection newCollection = new StrokeCollection();
            newCollection.Add(e.Stroke);
            targetCanvas.Strokes.Replace(tempStrokeCollection,newCollection);

            Console.WriteLine("After: Left Canvas Count -  {0}", canvasLeft.Strokes.Count);
            Console.WriteLine("After: Right Canvas Count -  {0}", canvasRight.Strokes.Count);

            tempStrokeCollection.Clear();
        }
        #endregion
    }
}