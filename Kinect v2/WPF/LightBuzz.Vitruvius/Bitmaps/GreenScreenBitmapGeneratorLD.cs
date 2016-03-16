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

using System;
using Microsoft.Kinect;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Provides extension methods for removing the background of a Kinect frame.
    /// This class provides a low definition image based on the depth frame
    /// which is 512x424 pixels on Kinect v2 and 320x200 on Kinect v1
    /// </summary>
    public class GreenScreenBitmapGeneratorLD : GreenScreenBitmapGenerator
    {
        #region Members

        /// <summary>
        /// The depth values.
        /// </summary>
        ushort[] _depthData = null;

        /// <summary>
        /// The body index values.
        /// </summary>
        byte[] _bodyData = null;

        /// <summary>
        /// The RGB pixel values.
        /// </summary>
        byte[] _colorData = null;

        /// <summary>
        /// The color points used for the background removal (green-screen) effect.
        /// </summary>
        ColorSpacePoint[] _colorPoints = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        public GreenScreenBitmapGeneratorLD()
        {
            isHD = false;
            if (CoordinateMapper == null)
            {
                CoordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        /// <param name="mapper">The coordinate mapper used for the background removal.</param>
        public GreenScreenBitmapGeneratorLD(CoordinateMapper mapper)
        {
            isHD = false;
            CoordinateMapper = mapper;
        }

        #endregion

        #region Methods

        private void InitBuffers(FrameDescription colorFrame, FrameDescription depthFrame, FrameDescription bodyIndexFrame)
        {
            int colorWidth = colorFrame.Width;
            int colorHeight = colorFrame.Height;

            int depthWidth = depthFrame.Width;
            int depthHeight = depthFrame.Height;

            int bodyIndexWidth = bodyIndexFrame.Width;
            int bodyIndexHeight = bodyIndexFrame.Height;

            _depthData = new ushort[depthWidth * depthHeight];
            _bodyData = new byte[depthWidth * depthHeight];
            _colorData = new byte[colorWidth * colorHeight * Constants.BYTES_PER_PIXEL];
            _colorPoints = new ColorSpacePoint[depthWidth * depthHeight];

            InitializeBitmap(depthFrame);
        }

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="depthFrame">The specified depth frame.</param>
        /// <param name="colorFrame">The specified color frame.</param>
        /// <param name="bodyIndexFrame">The specified body index frame.</param>
        override public void Update(ColorFrame colorFrame, DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame)
        {
            int colorWidth = colorFrame.FrameDescription.Width;
            int colorHeight = colorFrame.FrameDescription.Height;

            int depthWidth = depthFrame.FrameDescription.Width;
            int depthHeight = depthFrame.FrameDescription.Height;

            int bodyIndexWidth = bodyIndexFrame.FrameDescription.Width;
            int bodyIndexHeight = bodyIndexFrame.FrameDescription.Height;

            if (Bitmap == null)
            {
                InitBuffers(colorFrame.FrameDescription, depthFrame.FrameDescription, bodyIndexFrame.FrameDescription);
            }

            if (((depthWidth * depthHeight) == _depthData.Length) && ((colorWidth * colorHeight * Constants.BYTES_PER_PIXEL) == _colorData.Length) && ((bodyIndexWidth * bodyIndexHeight) == _bodyData.Length))
            {
                depthFrame.CopyFrameDataToArray(_depthData);

                if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                {
                    colorFrame.CopyRawFrameDataToArray(_colorData);
                }
                else
                {
                    colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Bgra);
                }

                bodyIndexFrame.CopyFrameDataToArray(_bodyData);

                CoordinateMapper.MapDepthFrameToColorSpace(_depthData, _colorPoints);

                Array.Clear(Pixels, 0, Pixels.Length);

                for (int y = 0; y < depthHeight; ++y)
                {
                    for (int x = 0; x < depthWidth; ++x)
                    {
                        int depthIndex = (y * depthWidth) + x;

                        if (_bodyData[depthIndex] != 0xff)
                        {
                            ColorSpacePoint colorPoint = _colorPoints[depthIndex];

                            int colorX = (int)(colorPoint.X + 0.5);
                            int colorY = (int)(colorPoint.Y + 0.5);

                            if ((colorX >= 0) && (colorX < colorWidth) && (colorY >= 0) && (colorY < colorHeight))
                            {
                                int colorIndex = ((colorY * colorWidth) + colorX) * Constants.BYTES_PER_PIXEL;
                                int displayIndex = depthIndex * Constants.BYTES_PER_PIXEL;

                                for (int b = 0; b < Constants.BYTES_PER_PIXEL; ++b)
                                {
                                    Pixels[displayIndex + b] = _colorData[colorIndex + b];
                                }
                            }
                        }
                    }
                }

                UpdateBitmap();
            }
        }

        #endregion
    }
}
