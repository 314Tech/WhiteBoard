using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawingCanvas
{   
    /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        // Undone strokes collection 
        private StrokeCollection _undoneCollection = new StrokeCollection();

        // Shared Strokes
        public SynchedStrokeCollection SynchedStrokes { get; set; }

        // Routed commands for key binding
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
        }

        #region Canvas Configuration
        protected void SetCanvasAttributes()
        {
            // Create an atribute group
            DrawingAttributes attr = new DrawingAttributes();
            attr = new DrawingAttributes();
            attr.Color = Colors.Blue;
            attr.Height = 20;
            attr.Width = 20;
            //attr.FitToCurve = false;

            canvasRight.DefaultDrawingAttributes = attr;
            canvasLeft.DefaultDrawingAttributes = attr;
        }

        private void ConfigureBindingCommands()
        {
            UndoCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            RedoCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            ClearCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
        }
        #endregion

        #region Actions
        // Undo the last stroke
        private void undo_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Undo count: {0}", canvasRight.Strokes.Count);
            int strokesCount = canvasRight.Strokes.Count;
            if (strokesCount > 0)
            {
                _undoneCollection.Add(canvasRight.Strokes[strokesCount - 1]);
                canvasRight.Strokes.RemoveAt(strokesCount - 1);
            }
        }

        // Redo last Undo operation
        private void redo_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Redo count: {0}", canvasLeft.Strokes.Count);
            int strokesCount = _undoneCollection.Count;
            if (strokesCount > 0)
            {
                canvasRight.Strokes.Insert(canvasRight.Strokes.Count, _undoneCollection[strokesCount - 1]);
                _undoneCollection.RemoveAt(strokesCount - 1);
            }
        }

        // Undo the last stroke
        private void clear_click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear");
            //Save all strokes before clearing the canvas. We can then redo the clear
            foreach (var stroke in canvasRight.Strokes.Reverse())
            {
                _undoneCollection.Add(stroke);
            }
            canvasRight.Strokes.Clear();
        }

        #endregion
    }
}