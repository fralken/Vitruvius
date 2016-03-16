//
// Copyright (c) LightBuzz Software.
// All rights reserved.
//
// http://lightbuzz.com
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
// AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Describes a generic bitmap generator.
    /// </summary>
    /// <typeparam name="T">The type of frame (<see cref="ColorFrame"/>, <see cref="DepthFrame"/>, <see cref="InfraredFrame"/>, <see cref="BodyIndexFrame"/>, etc).</typeparam>
    public abstract class BitmapGenerator<T>
    {
        /// <summary>
        /// Returns the RGB pixel values.
        /// </summary>
        public byte[] Pixels { get; private set; }

        /// <summary>
        /// Returns the width of the bitmap.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Returns the height of the bitmap.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Returns the actual bitmap.
        /// </summary>
        public WriteableBitmap Bitmap { get; protected set; }

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="frame">The specified Kinect frame.</param>
        public virtual void Update(T frame)
        {

        }

        /// <summary>
        /// Instance the internal bitmap that will contains a copy of a Kinect frame.
        /// It must be called once and only once.
        /// </summary>
        /// <param name="frameDescription">contains the frame's dimensions</param>
        protected void InitializeBitmap(FrameDescription frameDescription)
        {
            Width = frameDescription.Width;
            Height = frameDescription.Height;
            Pixels = new byte[Width * Height * Constants.BYTES_PER_PIXEL];
            Bitmap = new WriteableBitmap(Width, Height, Constants.DPI, Constants.DPI, Constants.FORMAT, null);
        }

        /// <summary>
        /// Call this in the end of the Update method to update the bitmap with the results of the current frame.
        /// This method copies the Pixels' buffer into the Bitmap's buffer.
        /// </summary>
        protected void UpdateBitmap()
        {
            Bitmap.Lock();

            Marshal.Copy(Pixels, 0, Bitmap.BackBuffer, Pixels.Length);
            Bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

            Bitmap.Unlock();
        }
    }
}
