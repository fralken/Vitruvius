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
    /// </summary>
    public abstract class GreenScreenBitmapGenerator : BitmapGenerator<ColorFrame>
    {
        #region Properties

        /// <summary>
        /// The coordinate mapper for the background removal (green-screen) effect.
        /// </summary>
        public CoordinateMapper CoordinateMapper { get; set; }

        /// <summary>
        /// true if the bitmap is in high resolution pixels
        /// </summary>
        public bool isHD { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        protected GreenScreenBitmapGenerator()
        {
            if (CoordinateMapper == null)
            {
                CoordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="GreenScreenBitmapGenerator"/>.
        /// </summary>
        /// <param name="mapper">The coordinate mapper used for the background removal.</param>
        protected GreenScreenBitmapGenerator(CoordinateMapper mapper)
        {
            CoordinateMapper = mapper;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Create a new instance of <see cref="GreenScreenBitmapGenerator"/>
        /// </summary>
        /// <param name="hD">
        /// true for a 1920x1080 bitmap (as the color frame), 
        /// false for a 512x424 bitmap (as the depth frame)
        /// </param>
        /// <returns></returns>
        public static GreenScreenBitmapGenerator Create(bool hD = true)
        {
            if (hD)
            {
                return new GreenScreenBitmapGeneratorHD();
            }
            else
            {
                return new GreenScreenBitmapGeneratorLD();
            }
        }

        /// <summary>
        /// Create a new instance of <see cref="GreenScreenBitmapGenerator"/>
        /// </summary>
        /// <param name="mapper">The coordinate mapper used for the background removal.</param>
        /// <param name="hD">
        /// true for a 1920x1080 bitmap (as the color frame), 
        /// false for a 512x424 bitmap (as the depth frame)
        /// </param>
        /// <returns></returns>
        public static GreenScreenBitmapGenerator Create(CoordinateMapper mapper, bool hD = true)
        {
            if (mapper == null)
            {
                return Create(hD);
            }
            else if (hD)
            {
                return new GreenScreenBitmapGeneratorHD(mapper);
            }
            else
            {
                return new GreenScreenBitmapGeneratorLD(mapper);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the bitmap with new frame data.
        /// </summary>
        /// <param name="depthFrame">The specified depth frame.</param>
        /// <param name="colorFrame">The specified color frame.</param>
        /// <param name="bodyIndexFrame">The specified body index frame.</param>
        abstract public void Update(ColorFrame colorFrame, DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame);

        #endregion
    }
}
