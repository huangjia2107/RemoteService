using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Video;
using Accord.Video.FFMPEG;
using SD = System.Drawing;

namespace ScreenCore.Capture
{
    public class ScreenCapture
    {
        private ScreenCaptureStream _captureStream = null;
        private VideoFileWriter _videoFileWriter = null;

        private SD.Rectangle _region;

        public ScreenCapture(int x, int y, int width, int height)
        {
            _region = new SD.Rectangle(x, y, width, height);

            _captureStream = new ScreenCaptureStream(_region, 40);
            _captureStream.NewFrame += NewFrameChanged;

            _videoFileWriter = new VideoFileWriter();
        }

        public void Start()
        {
            _videoFileWriter.Open("e:/temp.avi", _region.Width, _region.Right, 25, VideoCodec.MSMPEG4v3, 4000 * 1024);
            _captureStream.Start();
        }

        public void Stop()
        {
            _captureStream.Stop();

            _captureStream.NewFrame -= NewFrameChanged;
            _videoFileWriter.Close();
        }

        private void NewFrameChanged(object sender, NewFrameEventArgs e)
        {
            _videoFileWriter.WriteVideoFrame(e.Frame);
        }
    }
}
