namespace TankGame_MapEditor.Views
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Map view class
    /// </summary>
    public class MapView : BaseView
    {
        /// <summary>
        /// Rectangle picking cursor
        /// </summary>
        private List<Line> pickingCursor = new List<Line>
        {
            new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Colors.White), Visibility = Visibility.Collapsed, X1 = -10.0, X2 = 10.0, Y1 = 0.0, Y2 = 0.0 },
            new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Colors.White), Visibility = Visibility.Collapsed, X1 = 0.0, X2 = 0.0, Y1 = -10.0, Y2 = 10.0 },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MapView"/> class
        /// </summary>
        public MapView()
        {
            this.PutRectangle = new UICommand((parameter) => this.PutRectangleTask(parameter));
            this.PutSpawn = new UICommand((parameter) => this.PutSpawnTask(parameter));
            this.DeleteItem = new UICommand((parameter) => this.Elements.Remove((Shape)parameter));

            for (int x = 0; x < 740; x += 20)
            {
                this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(70, 70, 70)), X1 = x, X2 = x, Y1 = 0, Y2 = 540.0 });
                this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(50, 50, 50)), X1 = x + 10.0, X2 = x + 10.0, Y1 = 0, Y2 = 540.0 });
            }

            this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(0, 100, 255)), X1 = 370, X2 = 370, Y1 = 0, Y2 = 540.0 });

            for (int y = 0; y < 540; y += 20)
            {
                this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(70, 70, 70)), X1 = 0, X2 = 740, Y1 = y, Y2 = y });
                this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(50, 50, 50)), X1 = 0, X2 = 740, Y1 = y + 10.0, Y2 = y + 10.0 });
            }

            this.Elements.Add(new Line { StrokeThickness = 1.0, Stroke = new SolidColorBrush(Color.FromRgb(255, 100, 0)), X1 = 0, X2 = 740, Y1 = 270, Y2 = 270.0 });
            this.pickingCursor.ForEach(line => this.Elements.Add(line));
        }

        /// <summary>
        /// Gets new file command
        /// </summary>
        public UICommand DeleteItem { get; }

        /// <summary>
        /// Contains all elements of the map
        /// </summary>
        public ObservableCollection<Shape> Elements { get; } = new ObservableCollection<Shape>();

        /// <summary>
        /// Gets put rectangle
        /// </summary>
        public UICommand PutRectangle { get; }

        /// <summary>
        /// Gets put spawn
        /// </summary>
        public UICommand PutSpawn { get; }

        /// <summary>
        /// Open map from file
        /// </summary>
        /// <param name="fileName">File path</param>
        /// <returns>Map from file</returns>
        public static MapView OpenMap(string fileName)
        {
            MapView map = new MapView();

            using (FileStream stream = File.OpenRead(fileName))
            {
                int spawns = stream.ReadByte();
                int walls = stream.ReadByte();
                int details = stream.ReadByte();

                for (int spawn = 0; spawn < spawns; spawn++)
                {
                    Point location = new Point(((sbyte)stream.ReadByte() * 10.0) + 370.0, ((sbyte)stream.ReadByte() * 10.0) + 270.0);
                    byte[] angleBytes = new[] { (byte)stream.ReadByte(), (byte)stream.ReadByte() };
                    Array.Reverse(angleBytes);

                    SpawnView spawnData = new SpawnView { Angle = BitConverter.ToUInt16(angleBytes, 0), PlayerNumber = spawn };
                    map.Elements.Add(new Ellipse { Tag = spawnData, Margin = new Thickness(location.X - 20.0, 0.0, 0.0, location.Y - 20.0) });
                }

                for (int wall = 0; wall < walls; wall++)
                {
                    Rectangle wallElement = new Rectangle();
                    double left = ((sbyte)stream.ReadByte() * 10.0) + 370.0;
                    double bottom = ((sbyte)stream.ReadByte() * 10.0) + 270.0;
                    double right = ((sbyte)stream.ReadByte() * 10.0) + 370.0;
                    double top = ((sbyte)stream.ReadByte() * 10.0) + 270 - 0;
                    wallElement.Width = Math.Abs(right - left);
                    wallElement.Height = Math.Abs(top - bottom);
                    wallElement.Margin = new Thickness(left, 0.0, 0.0, bottom);

                    WallView wallData = new WallView();
                    byte flags = (byte)stream.ReadByte();
                    wallData.Type = flags == 1 ? 1 : (flags == 4 ? 2 : 0);
                    wallElement.Tag = wallData;
                    map.Elements.Add(wallElement);
                }
            }

            return map;
        }

        /// <summary>
        /// Save map to file
        /// </summary>
        /// <param name="fileName">File path</param>
        public void SaveMap(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (FileStream stream = File.OpenWrite(fileName))
            {
                List<Ellipse> spawns = this.Elements.OfType<Ellipse>().ToList();
                List<Rectangle> walls = this.Elements.OfType<Rectangle>().ToList();

                // Write header
                stream.Write(new byte[] { (byte)spawns.Count, (byte)walls.Count, 0 }, 0, 3);

                // Write spawns
                foreach (Ellipse spawn in spawns.OrderBy(spawn => ((SpawnView)spawn.Tag).PlayerNumber))
                {
                    byte[] bytes = SpawnView.ToBytes(new Point(spawn.Margin.Left + 20.0, spawn.Margin.Bottom + 20.0), (SpawnView)spawn.Tag);
                    stream.Write(bytes, 0, bytes.Length);
                }

                // Write spawns
                foreach (Rectangle wall in walls)
                {
                    byte[] bytes = WallView.ToBytes(wall.Margin.Left, wall.Margin.Bottom + wall.Height, wall.Margin.Left + wall.Width, wall.Margin.Bottom, (WallView)wall.Tag);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        /// <summary>
        /// Snap point to grid
        /// </summary>
        /// <param name="point">Point to snap</param>
        /// <param name="isSpawn">Limit spawn position</param>
        /// <returns>Snapped point</returns>
        internal static Point SnapToGrid(Point point, bool isSpawn)
        {
            double spawnSize = isSpawn ? 20.0 : 0.0;
            return new Point(
                Math.Round(Math.Min(Math.Max(point.X, spawnSize), 740.0 - spawnSize) / 10.0) * 10.0,
                540.0 - (Math.Round(Math.Min(Math.Max(point.Y, spawnSize), 540.0 - spawnSize) / 10.0) * 10.0));
        }

        /// <summary>
        /// Invoke action on main window dispatcher
        /// </summary>
        /// <param name="action">Action to invoke</param>
        private static void DispatcherInvoke(Action action)
        {
            if (App.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                App.Current.Dispatcher.Invoke(() => action());
            }
        }

        /// <summary>
        /// Create rectangle for edditing and add it to the list of elements
        /// </summary>
        /// <param name="bottomLeft">Bottom left corner of the rectangle</param>
        /// <returns>Rectangle, or <see langword="null"/> if full</returns>
        private Rectangle CreateAndAddRectangle(Point bottomLeft)
        {
            if (Elements.Count(element => element is Rectangle) >= byte.MaxValue)
            {
                return null;
            }

            Rectangle rectangle = new Rectangle
            {
                Width = 0.0,
                Height = 0.0,
                Margin = new Thickness(bottomLeft.X, 0.0, 0.0, bottomLeft.Y),
                Tag = new WallView()
            };

            this.Elements.Add(rectangle);
            return rectangle;
        }

        /// <summary>
        /// Create spawn for edditing and add it to the list of elements
        /// </summary>
        /// <param name="bottomLeft">center of spawn</param>
        /// <returns>Spawn, or <see langword="null"/> if full</returns>
        private Ellipse CreateAndAddSpawn(Point center)
        {
            if (Elements.Count(element => element is Ellipse) >= byte.MaxValue)
            {
                return null;
            }

            Ellipse spawn = new Ellipse
            {
                Margin = new Thickness(center.X - 20.0, 0.0, 0.0, center.Y - 20.0),
                Tag = new SpawnView()
            };

            this.Elements.Add(spawn);
            return spawn;
        }

        /// <summary>
        /// Put rectangle on map
        /// </summary>
        /// <param name="mapGrid">map grid</param>
        private void PutRectangleTask(object mapGrid)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                Point currentMouseLocation = new Point();
                Point pickedLocation = new Point();
                ConcurrentQueue<EventArgs> actions = new ConcurrentQueue<EventArgs>();
                Rectangle rectangle = null;
                bool cancel = false;
                bool firstPicked = false;

                MouseButtonEventHandler mouseClicked = (sender, e) => actions.Enqueue(e);
                MouseEventHandler mouseMoved = (sender, e) => currentMouseLocation = MapView.SnapToGrid(e.GetPosition((ItemsControl)mapGrid), false);
                EventHandler taskCanceled = (sender, e) => actions.Enqueue(e);

                MapView.DispatcherInvoke(() =>
                {
                    ((ItemsControl)mapGrid).CaptureMouse();
                    currentMouseLocation = MapView.SnapToGrid(Mouse.GetPosition((ItemsControl)mapGrid), false);
                    rectangle = this.CreateAndAddRectangle(currentMouseLocation);
                    ((MainWindow)App.Current.MainWindow).EditorMouseClicked += mouseClicked;
                    ((MainWindow)App.Current.MainWindow).EditorMouseMoving += mouseMoved;

                    // Show cursor and set its starting position
                    this.pickingCursor.ForEach(line => line.Visibility = Visibility.Visible);
                    this.SetcursorPosition(currentMouseLocation);

                    ((MainWindow)App.Current.MainWindow).PickerMessage.Content = "Pick first point...";
                });

                while (!cancel)
                {
                    MapView.DispatcherInvoke(
                        () =>
                        {
                            if (Keyboard.GetKeyStates(Key.Escape) == KeyStates.Down)
                            {
                                cancel = true;
                                this.Elements.Remove(rectangle);
                            }
                        });
                    
                    if (!cancel && actions.Any())
                    {
                        EventArgs action;

                        if (!actions.TryDequeue(out action))
                        {
                            continue;
                        }

                        if (action is MouseButtonEventArgs)
                        {
                            if (((MouseButtonEventArgs)action).ChangedButton == MouseButton.Left && ((MouseButtonEventArgs)action).ButtonState == MouseButtonState.Released)
                            {
                                if (!firstPicked)
                                {
                                    firstPicked = true;
                                }
                                else
                                {
                                    cancel = true;
                                }
                            }
                            else if (((MouseButtonEventArgs)action).ChangedButton == MouseButton.Middle && ((MouseButtonEventArgs)action).ButtonState == MouseButtonState.Released)
                            {
                                cancel = true;
                                MapView.DispatcherInvoke(
                                        () =>
                                        {
                                            this.Elements.Remove(rectangle);
                                        });
                            }
                        }
                    }
                    else
                    {
                        MapView.DispatcherInvoke(() => this.SetcursorPosition(currentMouseLocation));

                        if (firstPicked)
                        {
                            MapView.DispatcherInvoke(
                                () =>
                                {
                                    double right = Math.Max(currentMouseLocation.X, pickedLocation.X);
                                    double left = Math.Min(currentMouseLocation.X, pickedLocation.X);
                                    double top = Math.Max(currentMouseLocation.Y, pickedLocation.Y);
                                    double bottom = Math.Min(currentMouseLocation.Y, pickedLocation.Y);

                                    rectangle.Width = Math.Max(right - left, 10.0);
                                    rectangle.Height = Math.Max(top - bottom, 10.0);
                                    rectangle.Margin = new Thickness(left, 0.0, 0.0, bottom);

                                    ((MainWindow)App.Current.MainWindow).PickerMessage.Content = string.Format("Pick second point... ({0},{1})", (int)rectangle.Width, (int)rectangle.Height);
                                });
                        }
                        else
                        {
                            pickedLocation = currentMouseLocation;

                            MapView.DispatcherInvoke(
                                () =>
                                {
                                    rectangle.Margin = new Thickness(currentMouseLocation.X, 0.0, 0.0, currentMouseLocation.Y);
                                });
                        }
                    }

                    Thread.Sleep(15);
                }

                // End task
                MapView.DispatcherInvoke(
                    () =>
                    {
                        ((MainWindow)App.Current.MainWindow).PickerMessage.Content = string.Empty;
                        this.pickingCursor.ForEach(line => line.Visibility = Visibility.Collapsed);
                        ((MainWindow)App.Current.MainWindow).EditorMouseClicked -= mouseClicked;
                        ((MainWindow)App.Current.MainWindow).EditorMouseMoving -= mouseMoved;
                        ((ItemsControl)mapGrid).ReleaseMouseCapture();
                    });
            }).Start();
        }

        /// <summary>
        /// Put rectangle on map
        /// </summary>
        /// <param name="mapGrid">map grid</param>
        private void PutSpawnTask(object mapGrid)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                Point currentMouseLocation = new Point();
                Point pickedLocation = new Point();
                ConcurrentQueue<EventArgs> actions = new ConcurrentQueue<EventArgs>();
                Ellipse spawn = null;
                bool cancel = false;

                MouseButtonEventHandler mouseClicked = (sender, e) => actions.Enqueue(e);
                MouseEventHandler mouseMoved = (sender, e) => currentMouseLocation = MapView.SnapToGrid(e.GetPosition((ItemsControl)mapGrid), true);
                EventHandler taskCanceled = (sender, e) => actions.Enqueue(e);

                MapView.DispatcherInvoke(() =>
                {
                    ((ItemsControl)mapGrid).CaptureMouse();
                    currentMouseLocation = MapView.SnapToGrid(Mouse.GetPosition((ItemsControl)mapGrid), true);
                    spawn = this.CreateAndAddSpawn(currentMouseLocation);
                    ((MainWindow)App.Current.MainWindow).EditorMouseClicked += mouseClicked;
                    ((MainWindow)App.Current.MainWindow).EditorMouseMoving += mouseMoved;

                    ((MainWindow)App.Current.MainWindow).PickerMessage.Content = "Pick point...";
                });

                while (!cancel)
                {

                    MapView.DispatcherInvoke(
                        () =>
                        {
                            if (Keyboard.GetKeyStates(Key.Escape) == KeyStates.Down)
                            {
                                cancel = true;
                                this.Elements.Remove(spawn);
                            }
                        });
                    
                    if (!cancel && actions.Any())
                    {
                        EventArgs action;

                        if (!actions.TryDequeue(out action))
                        {
                            continue;
                        }

                        if (action is MouseButtonEventArgs)
                        {
                            if (((MouseButtonEventArgs)action).ChangedButton == MouseButton.Left && ((MouseButtonEventArgs)action).ButtonState == MouseButtonState.Released)
                            {
                                cancel = true;
                            }
                            else if (((MouseButtonEventArgs)action).ChangedButton == MouseButton.Middle && ((MouseButtonEventArgs)action).ButtonState == MouseButtonState.Released)
                            {
                                cancel = true;
                                MapView.DispatcherInvoke(
                                        () =>
                                        {
                                            this.Elements.Remove(spawn);
                                        });
                            }
                        }
                    }
                    else
                    {
                        pickedLocation = currentMouseLocation;

                        MapView.DispatcherInvoke(
                            () =>
                            {
                                spawn.Margin = new Thickness(currentMouseLocation.X - 20.0, 0.0, 0.0, currentMouseLocation.Y - 20.0);
                            });
                    }

                    Thread.Sleep(15);
                }

                // End task
                MapView.DispatcherInvoke(
                    () =>
                    {
                        ((MainWindow)App.Current.MainWindow).PickerMessage.Content = string.Empty;
                        ((MainWindow)App.Current.MainWindow).EditorMouseClicked -= mouseClicked;
                        ((MainWindow)App.Current.MainWindow).EditorMouseMoving -= mouseMoved;
                        ((ItemsControl)mapGrid).ReleaseMouseCapture();
                    });
            }).Start();
        }

        /// <summary>
        /// Set cursor position
        /// </summary>
        /// <param name="position">New position</param>
        private void SetcursorPosition(Point position)
        {
            this.pickingCursor[0].X1 = position.X - 10.0;
            this.pickingCursor[0].X2 = position.X + 10.0;
            this.pickingCursor[0].Y1 = 540.0 - position.Y;
            this.pickingCursor[0].Y2 = 540.0 - position.Y;
            this.pickingCursor[1].X1 = position.X;
            this.pickingCursor[1].X2 = position.X;
            this.pickingCursor[1].Y1 = 540.0 - (position.Y - 10.0);
            this.pickingCursor[1].Y2 = 540.0 - (position.Y + 10.0);
        }

        /// <summary>
        /// Map header file
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct AABB
        {
            public sbyte Left;
            public sbyte Bottom;
            public sbyte Right;
            public sbyte Top;
        }

        /// <summary>
        /// Map header file
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Header
        {
            public byte NumOfSpawns;
            public byte NumOfWalls;
            public byte NumOfDetails;
        }
    }
}