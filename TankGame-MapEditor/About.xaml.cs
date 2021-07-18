namespace TankGame_MapEditor
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for about dialog
    /// </summary>
    public partial class About : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class
        /// </summary>
        public About()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Open clicked link
        /// </summary>
        /// <param name="sender">Hyper link element</param>
        /// <param name="e">Hyper link navigation event</param>
        private void OpenLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}