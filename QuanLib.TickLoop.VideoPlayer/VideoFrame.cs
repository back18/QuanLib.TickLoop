﻿using QuanLib.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.VideoPlayer
{
    public class VideoFrame<TPixel> : UnmanagedBase where TPixel : unmanaged, IPixel<TPixel>
    {
        public VideoFrame(Image<TPixel> image, TimeSpan position)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            Image = image;
            Position = position;
        }

        public Image<TPixel> Image { get; }

        public TimeSpan Position { get; }

        protected override void DisposeUnmanaged()
        {
            Image.Dispose();
        }
    }
}
