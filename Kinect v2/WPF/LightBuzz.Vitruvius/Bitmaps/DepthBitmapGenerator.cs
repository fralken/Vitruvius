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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Creates the bitmap representation of a Kinect depth frame.
    /// </summary>
    public class DepthBitmapGenerator : BitmapGenerator<DepthFrame>
    {
        #region Properties

        /// <summary>
        /// Returns the current depth values.
        /// </summary>
        public ushort[] DepthData { get; protected set; }

        /// <summary>
        /// Returns the body index values.
        /// </summary>
        public byte[] BodyData { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="frame">The specified Kinect depth frame.</param>
        public override void Update(DepthFrame frame)
        {
            UpdateData(frame, null);
        }

        /// <summary>
        /// Updates the bitmap with new frame data and highlights the players.
        /// </summary>
        /// <param name="depthFrame">The specified Kinect depth frame.</param>
        /// <param name="bodyIndexFrame">The specified Kinect body index frame.</param>
        public void Update(DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame)
        {
            UpdateData(depthFrame, bodyIndexFrame);
        }

        private void UpdateData(DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame)
        {
            ushort minDepth = depthFrame.DepthMinReliableDistance;
            ushort maxDepth = depthFrame.DepthMaxReliableDistance;

            if (DepthData == null)
            {
                if (Bitmap == null)
                {
                    InitializeBitmap(depthFrame.FrameDescription);
                }
                DepthData = new ushort[Width * Height];
            }

            depthFrame.CopyFrameDataToArray(DepthData);
            if (bodyIndexFrame != null)
            {
                if (BodyData == null)
                {
                    BodyData = new byte[Width * Height];
                }
                bodyIndexFrame.CopyFrameDataToArray(BodyData);
            }

            // Convert the depth to RGB.
            int colorIndex = 0;

            for (int depthIndex = 0; depthIndex < DepthData.Length; ++depthIndex)
            {
                // Get the depth for this pixel
                ushort depth = DepthData[depthIndex];

                // To convert to a byte, we clamp the value between minDepth and maxDepth,
                // then we subtract minDepth so that the range is between 0 and (maxDepth - minDepth),
                // then we scale to fit in one byte, then we reverse the value so that higher values
                // correspond to smaller distances
                ushort clamped = (ushort)(depth < minDepth ? minDepth : (depth > maxDepth ? maxDepth : depth));
                byte intensity = (byte)((float)(clamped - minDepth) / (float)(maxDepth - minDepth) * 255);
                intensity = (byte)(255 - intensity);

                byte intensityB = intensity, intensityG = intensity, intensityR = intensity;

                if (bodyIndexFrame != null)
                {
                    byte player = BodyData[depthIndex];
                    if (player != 0xff)
                    {
                        player++;
                        if ((player & 1) == 0) intensityB = 0;
                        if ((player & 2) == 0) intensityG = 0;
                        if ((player & 4) == 0) intensityR = 0;
                    }
                }

                Pixels[colorIndex++] = intensityB; // Blue
                Pixels[colorIndex++] = intensityG; // Green
                Pixels[colorIndex++] = intensityR; // Red
                Pixels[colorIndex++] = 0xff; // Alpha
            }

            UpdateBitmap();
        }

        #endregion
    }
}
