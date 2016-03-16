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
    /// Creates the bitmap representation of a Kinect infrared frame.
    /// </summary>
    public class InfraredBitmapGenerator : BitmapGenerator<InfraredFrame>
    {
        #region Properties

        /// <summary>
        /// Returns the current infrared values.
        /// </summary>
        public ushort[] InfraredData { get; protected set; }

        /// <summary>
        /// Number of least significant bits to be discarded from infrared data
        /// The less discarded, the more details are preserved in the background,
        /// but less in the foreground
        /// </summary>
        public byte DiscardedLsb {
            get { return _discardedLsb; } 
            set { _discardedLsb = value > MaxDiscardableLsb ? MaxDiscardableLsb : value; }
        }
        private byte _discardedLsb = 6;

        /// <summary>
        /// Max number of least significant bits that can be discarded. 
        /// Since infrared data is 16 bit wide, and we need to map it to 8 bit,
        /// we can at most discard 8 bit.
        /// </summary>
        public static byte MaxDiscardableLsb { get { return 8; } }
        
        #endregion

        #region Methods

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="frame">The specified Kinect infrared frame.</param>
        public override void Update(InfraredFrame frame)
        {
            if (Bitmap == null)
            {
                InitializeBitmap(frame.FrameDescription);
                InfraredData = new ushort[Width * Height];
            }

            frame.CopyFrameDataToArray(InfraredData);

            // Convert the infrared to RGB.
            ushort maxInfra = (ushort)((1 << (8 + _discardedLsb)) - 1);
            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < InfraredData.Length; infraredIndex++)
            {
                // Get the infrared value for this pixel
                ushort ir = InfraredData[infraredIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits so that we preserve details
                byte intensity = (byte)((ir > maxInfra ? maxInfra : ir) >> _discardedLsb);

                Pixels[colorIndex++] = intensity; // Blue
                Pixels[colorIndex++] = intensity; // Green   
                Pixels[colorIndex++] = intensity; // Red
                Pixels[colorIndex++] = 0xff; // Alpha
            }

            UpdateBitmap();
        }

        #endregion
    }
}
