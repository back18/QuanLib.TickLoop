using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.VideoPlayer.Events
{
    public class VideoFrameChangedEventArgs<TPixel> : EventArgs where TPixel : unmanaged, IPixel<TPixel>
    {
        public VideoFrameChangedEventArgs(VideoFrame<TPixel>? oldVideoFrame, VideoFrame<TPixel>? newVideoFrame)
        {
            OldVideoFrame = oldVideoFrame;
            NewVideoFrame = newVideoFrame;
        }

        public VideoFrame<TPixel>? OldVideoFrame { get; }

        public VideoFrame<TPixel>? NewVideoFrame { get; }
    }
}
