using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace TankGame_MapEditor.Views
{
    public class SpawnView : BaseView
    {
        private ushort angle;

        private int playerNumber;

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