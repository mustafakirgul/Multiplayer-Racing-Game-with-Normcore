using System;
using System.Runtime.InteropServices;

namespace Trail
{
    /// <summary>
    /// Resolution class used by Trail to get or set new resolutions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Resolution
    {
        /// <summary>
        /// The resolution width.
        /// </summary>
        public int Width;
        
        /// <summary>
        /// The resolution height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Return whether height is larger than width.
        /// </summary>
        public bool IsPortrait { get { return Height > Width; } }
        
        /// <summary>
        /// Returns whether width is larger than height. 
        /// </summary>
        public bool IsLandscape { get { return Width > Height; } }

        /// <summary>
        /// Returns aspect ratio of the resolution, Width / Height.
        /// </summary>
        public float AspectRatio { get { return (float)Width / (float)Height; } }

        /// <summary>
        /// Returns a new resolution with height and width set to 0
        /// </summary>
        public static Resolution Zero { get { return new Resolution(0, 0); } }

        public Resolution(UnityEngine.Resolution other)
        {
            this.Width = other.width;
            this.Height = other.height;
        }

        public Resolution(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public override string ToString()
        {
            return string.Format("{0}x{1}", this.Width, this.Height);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Resolution)
            {
                var other = (Resolution)obj;
                return this.Width == other.Width && this.Height == other.Height;
            }
            return false;
        }

        public static bool operator ==(Resolution lhs, Resolution rhs)
        {
            return lhs.Width == rhs.Width && lhs.Height == rhs.Height;
        }

        public static bool operator !=(Resolution lhs, Resolution rhs)
        {
            return !(lhs == rhs);
        }
    }

}
