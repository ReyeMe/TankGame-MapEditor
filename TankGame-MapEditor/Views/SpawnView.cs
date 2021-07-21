namespace TankGame_MapEditor.Views
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Player spawn
    /// </summary>
    public class SpawnView : BaseView
    {
        /// <summary>
        /// Player start angle
        /// </summary>
        private ushort angle;

        /// <summary>
        /// PLayer spawn number
        /// </summary>
        private int playerNumber;

        /// <summary>
        /// Gets or sets player start angle
        /// </summary>
        public ushort Angle
        {
            get
            {
                return this.angle;
            }

            set
            {
                this.angle = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets player color
        /// </summary>
        public SolidColorBrush PlayerColor
        {
            get
            {
                switch (this.PlayerNumber)
                {
                    case 0:
                        return new SolidColorBrush(Colors.Crimson);

                    case 1:
                        return new SolidColorBrush(Colors.DeepSkyBlue);

                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }
        }

        /// <summary>
        /// Gets or sets player start number
        /// </summary>
        public int PlayerNumber
        {
            get
            {
                return this.playerNumber;
            }

            set
            {
                this.playerNumber = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(SpawnView.PlayerColor));
            }
        }

        /// <summary>
        /// Convert spawn object to byte array
        /// </summary>
        /// <param name="location">Spawn location</param>
        /// <param name="spawn">Spawn object</param>
        /// <returns>Byte array</returns>
        public static byte[] ToBytes(Point location, SpawnView spawn)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(unchecked((byte)((sbyte)((location.X - 370.0) / 10.0))));
            bytes.Add(unchecked((byte)((sbyte)((location.Y - 270.0) / 10.0))));
            byte[] a = BitConverter.GetBytes(spawn.Angle);
            Array.Reverse(a);
            bytes.AddRange(a);
            return bytes.ToArray();
        }
    }
}