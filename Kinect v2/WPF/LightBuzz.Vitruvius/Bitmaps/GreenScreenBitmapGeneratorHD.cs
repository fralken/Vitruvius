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

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Provides extension methods for removing the background of a Kinect frame.
    /// This class provides a high definition image based on the color frame
    /// which is 1920x1080 pixels on Kinect v2 and 640x480 on Kinect v1
    /// </summary>
    public class GreenScreenBitmapGeneratorHD : GreenScreenBitmapGenerator
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
        /// The color to depth mapping points used for the background removal (green-screen) effect.
        /// </summary>
        DepthSpacePoint[] _depthPoints = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        public GreenScreenBitmapGeneratorHD()
        {
            isHD = true;
            if (CoordinateMapper == null)
            {
                CoordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        /// <param name="mapper">The coordinate mapper used for the background removal.</param>
        public GreenScreenBitmapGeneratorHD(CoordinateMapper mapper)
        {
            isHD = true;
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
            _depthPoints = new DepthSpacePoint[colorWidth * colorHeight];

            InitializeBitmap(colorFrame);
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

            if (((depthWidth * depthHeight) == _depthData.Length) && ((colorWidth * colorHeight * Constants.BYTES_PER_PIXEL) == Pixels.Length) && ((bodyIndexWidth * bodyIndexHeight) == _bodyData.Length))
            {
                depthFrame.CopyFrameDataToArray(_depthData);

                if (colorFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                {
                    colorFrame.CopyRawFrameDataToArray(Pixels);
                }
                else
                {
                    colorFrame.CopyConvertedFrameDataToArray(Pixels, ColorImageFormat.Bgra);
                }

                bodyIndexFrame.CopyFrameDataToArray(_bodyData);

                CoordinateMapper.MapColorFrameToDepthSpace(_depthData, _depthPoints);

                // Loop over each row and column of the color image
                // Zero out any pixels that don't correspond to a body index
                for (int i = 0, ci = 0; i < _depthPoints.Length; ++i, ci += Constants.BYTES_PER_PIXEL)
                {
                    float colorToDepthX = _depthPoints[i].X;
                    float colorToDepthY = _depthPoints[i].Y;

                    // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                    if (!float.IsNegativeInfinity(colorToDepthX) &&
                        !float.IsNegativeInfinity(colorToDepthY))
                    {
                        // Make sure the depth pixel maps to a valid point in color space
                        int depthX = (int)(colorToDepthX + 0.5f);
                        int depthY = (int)(colorToDepthY + 0.5f);

                        // If the point is not valid, there is no body index there.
                        if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) && (depthY < depthHeight))
                        {
                            int depthIndex = (depthY * depthWidth) + depthX;

                            // If we are tracking a body for the current pixel, do not zero out the pixel
                            if (_bodyData[depthIndex] != 0xff)
                            {
                                continue;
                            }
                        }
                    }

                    for (int b = 0; b < Constants.BYTES_PER_PIXEL; ++b)
                    {
                        Pixels[ci + b] = 0;
                    }
                }

                UpdateBitmap();
            }
        }

        #endregion
    }
}
