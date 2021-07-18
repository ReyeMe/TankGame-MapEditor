namespace TankGame_MapEditor
{
    using System.Windows;
    using System.Windows.Input;
    using TankGame_MapEditor.Views;

    /// <summary>
    /// Main editor window interaction logic
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class
        /// </summary>
        public MainWindow()
        {
            this.DataContext = new Views.MainView();
            this.InitializeComponent();

            this.SourceInitialized += (s, e) =>
            {
                this.MinWidth = this.ActualWidth;
                this.MinHeight = this.ActualHeight;
            };
        }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event MouseButtonEventHandler EditorMouseClicked;

        /// <summary>
        /// Property changed event
        /// </summary>
        public event MouseEventHandler EditorMouseMoving;

        /// <summary>
        /// Map area mouse is moving
        /// </summary>
        /// <param name="sender">Map area grid</param>
        /// <param name="e">Mouse event</param>
        private void MapAreaMouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = MapView.SnapToGrid(Mouse.GetPosition(this.MapArea), false);
            this.MouseX.Content = ((int)mouse.X).ToString();
            this.MouseY.Content = ((int)mouse.Y).ToString();
            this.EditorMouseMoving?.Invoke(sender, e);
        }

        /// <summary>
        /// Map area mouse was clicked
        /// </summary>
        /// <param name="sender">Map area grid</param>
        /// <param name="e">Mouse event</param>
        private void MapAreaMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.EditorMouseClicked?.Invoke(sender, e);
        }

        /// <summary>
        /// Open about dialog
        /// </summary>
        /// <param name="sender">Button control</param>
        /// <param name="e">Empty event</param>
        private void OpenAboutDialog(object sender, RoutedEventArgs e)
        {
            new About { Owner = this }.ShowDialog();
        }
    }
}