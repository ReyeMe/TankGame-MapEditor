namespace TankGame_MapEditor.Views
{
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Main editor view model
    /// </summary>
    public class MainView : BaseView
    {
        /// <summary>
        /// Map data
        /// </summary>
        private MapView map = new MapView();

        /// <summary>
        /// Current map file
        /// </summary>
        private string mapFile = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class
        /// </summary>
        public MainView()
        {
            this.NewFile = new UICommand((parameter) => this.MapCreate());
            this.OpenFile = new UICommand((parameter) => this.MapOpen());
            this.SaveFile = new UICommand((parameter) => this.MapSave());
            this.SaveFileAs = new UICommand((parameter) => this.MapSaveAs());

            this.PutRectangle = new UICommand((parameter) => this.map.PutRectangle.Execute(parameter));
            this.PutSpawn = new UICommand((parameter) => this.map.PutSpawn.Execute(parameter));
            this.DeleteItem = new UICommand((parameter) => this.map.DeleteItem.Execute(parameter));
            this.CheckMap = new UICommand((parameter) => this.CheckMapForErrors());
        }

        /// <summary>
        /// Checks current map for errors
        /// </summary>
        private void CheckMapForErrors()
        {
            IEnumerable<SpawnView> spawns = this.Map.Elements.Select(element => element.Tag).OfType<SpawnView>();
            List<string> errors = new List<string>();

            // Check spawn points
            if (spawns.Count(spawn => spawn.PlayerNumber == 0) > 1)
            {
                errors.Add("First player can have only single spawn point!");
            }

            if (spawns.Count(spawn => spawn.PlayerNumber == 1) > 1)
            {
                errors.Add("Second player can have only single spawn point!");
            }

            if (spawns.Count(spawn => spawn.PlayerNumber == 0) == 0)
            {
                errors.Add("First player is missing a spawn point!");
            }

            if (spawns.Count(spawn => spawn.PlayerNumber == 1) == 0)
            {
                errors.Add("Second player is missing a spawn point!");
            }

            // TODO: Check for geometry intersection

            // Show result
            if (errors.Any())
            {
                MessageBox.Show(
                    App.Current.MainWindow,
                    "Found errors:\n" + string.Join("\n", errors.ToArray()), 
                    "Error check", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    App.Current.MainWindow, 
                    "Ok.",
                    "Error check",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Gets or sets map data
        /// </summary>
        public MapView Map
        {
            get
            {
                return this.map;
            }

            set
            {
                this.map = value;
                this.RaisePropertyChanged();

                Binding binding = new Binding("Elements");
                binding.Source = this.map;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                ((MainWindow)App.Current.MainWindow).MapArea.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            }
        }

        /// <summary>
        /// Gets or sets map file name
        /// </summary>
        public string MapFile
        {
            get
            {
                return this.mapFile;
            }

            set
            {
                this.mapFile = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets new file command
        /// </summary>
        public UICommand DeleteItem { get; }

        /// <summary>
        /// Gets put rectangle
        /// </summary>
        public UICommand PutRectangle { get; }

        /// <summary>
        /// Gets put spawn
        /// </summary>
        public UICommand PutSpawn { get; }

        /// <summary>
        /// Gets new file command
        /// </summary>
        public UICommand NewFile { get; }

        /// <summary>
        /// Gets open file command
        /// </summary>
        public UICommand OpenFile { get; }

        /// <summary>
        /// Gets save file command
        /// </summary>
        public UICommand SaveFile { get; }

        /// <summary>
        /// Gets save file as command
        /// </summary>
        public UICommand SaveFileAs { get; }

        /// <summary>
        /// Gets check map command
        /// </summary>
        public UICommand CheckMap { get; }

        /// <summary>
        /// Map create action
        /// </summary>
        private void MapCreate()
        {
            if (!this.MapSaveMessage())
            {
                return;
            }

            this.MapFile = string.Empty;
            this.Map = new MapView();
        }

        /// <summary>
        /// Map open action
        /// </summary>
        private void MapOpen()
        {
            if (!this.MapSaveMessage())
            {
                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog { Filter = "Tank game level|*.TGL", Title = "Open level" };

            if (fileDialog.ShowDialog().Value)
            {
                this.MapFile = fileDialog.FileName;
                this.Map = MapView.OpenMap(fileDialog.FileName);
            }
        }

        /// <summary>
        /// Map save action
        /// </summary>
        private bool MapSave()
        {
            if (string.IsNullOrWhiteSpace(this.MapFile) || !File.Exists(this.MapFile) || File.GetAttributes(this.MapFile).HasFlag(FileAttributes.ReadOnly))
            {
                return this.MapSaveAs();
            }

            this.Map.SaveMap(this.MapFile);
            return true;
        }

        /// <summary>
        /// Map save as action
        /// </summary>
        private bool MapSaveAs()
        {
            SaveFileDialog fileDialog = new SaveFileDialog { Filter = "Tank game level|*.TGL", Title = "Save level", OverwritePrompt = true };

            if (fileDialog.ShowDialog().Value)
            {
                this.MapFile = fileDialog.FileName;

                if (!File.Exists(this.MapFile) || !File.GetAttributes(this.MapFile).HasFlag(FileAttributes.ReadOnly))
                {
                    this.Map.SaveMap(this.MapFile);
                    return true;
                }
                else
                {
                    MessageBox.Show(App.Current.MainWindow, "Saving level failed. File is read-only!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Show message to the user asking him if he wants to save the map
        /// </summary>
        /// <returns>Return true if task not canceled</returns>
        private bool MapSaveMessage()
        {
            MessageBoxResult result = MessageBox.Show(App.Current.MainWindow, "Do you want to save current work?", "Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return this.MapSave();

                case MessageBoxResult.No:
                    return true;

                default:
                    return false;
            }
        }
    }
}