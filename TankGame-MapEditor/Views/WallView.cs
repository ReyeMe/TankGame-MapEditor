using System;
using System.Collections.Generic;

namespace TankGame_MapEditor.Views
{
    /// <summary>
    /// Basic wall view
    /// </summary>
    public class WallView : BaseView
    {
        /// <summary>
        /// Wall type
        /// </summary>
        private WallType type = WallType.Solid;

        /// <summary>
        /// Wall type
        /// </summary>
        public enum WallType
        {
            /// <summary>
            /// Solid wall
            /// </summary>
            Solid = 0,

            /// <summary>
            /// Breakable wall
            /// </summary>
            Breakable,

            /// <summary>
            /// Bullet can go through
            /// </summary>
            IgnoreBullet
        }

        /// <summary>
        /// Gets or sets wall type
        /// </summary>
        public int Type
        {
            get
            {
                return (int)this.type;
            }

            set
            {
                this.type = (WallType)value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Convert wall object to bytes
        /// </summary>
        /// <param name="left">Left coordinate</param>
        /// <param name="top">Top coordinate</param>
        /// <param name="right">Right coordinate</param>
        /// <param name="bottom">Bottom coordinate</param>
        /// <param name="wall">Wall object</param>
        /// <returns>Converted bytes</returns>
        public static byte[] ToBytes(double left, double top, double right, double bottom, WallView wall)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(WallView.GetByte(left - 370.0));
            bytes.Add(WallView.GetByte(bottom - 270.0));
            bytes.Add(WallView.GetByte(right - 370.0));
            bytes.Add(WallView.GetByte(top - 270.0));
            bytes.Add((byte)((wall.type == WallType.Breakable ? 1 : 0) | (wall.type == WallType.IgnoreBullet ? 4 : 0)));
            return bytes.ToArray();
        }

        /// <summary>
        /// Get byte from double value
        /// </summary>
        /// <param name="location">Double value</param>
        /// <returns>Converted value</returns>
        private static byte GetByte(double location)
        {
            byte[] bytes = BitConverter.GetBytes((sbyte)(location / 10.0));
            return bytes[0];
        }
    }
}