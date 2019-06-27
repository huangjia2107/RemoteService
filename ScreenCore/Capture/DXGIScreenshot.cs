using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ScreenCore.Interface;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace ScreenCore.Capture
{
    class DXGIScreenshot : IScreenshot
    {
        // # of graphics card adapter
        const int numAdapter = 0;

        // # of output device (i.e. monitor)
        const int numOutput = 0;

        Factory1 factory = null;
        Adapter1 adapter = null;
        Device device = null;

        Output output = null;
        Output1 output1 = null;

        Texture2D screenTexture = null;
        OutputDuplication duplicatedOutput;

        public DXGIScreenshot()
        {
            // Create DXGI Factory1
            factory = new Factory1();
            adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            output = adapter.GetOutput(numOutput);
            output1 = output.QueryInterface<Output1>();

            // Width/Height of desktop to capture
            this.Width = ((Rectangle)output.Description.DesktopBounds).Width;
            this.Height = ((Rectangle)output.Description.DesktopBounds).Height;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = Width,
                Height = Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);
        }

        private byte[] GetBitmap()
        {
            byte[] buffer = null;

            bool captureDone = false;
            for (int i = 0; !captureDone; i++)
            {
                try
                {
                    SharpDX.DXGI.Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                    if (i > 0)
                    {
                        // copy resource into memory that can be accessed by the CPU
                        using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                            device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                        // Get the desktop capture texture
                        var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);

                        buffer = new byte[Width * Height * 4];
                        Marshal.Copy(mapSource.DataPointer, buffer, 0, buffer.Length);
                         
                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                        // Capture done
                        captureDone = true;
                    }

                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                     
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                }
            }

            return buffer;
        }

        public void Dispose()
        {
            duplicatedOutput.Dispose();
            screenTexture.Dispose();
            output1.Dispose();
            output.Dispose();
            device.Dispose();
            adapter.Dispose();
            factory.Dispose();
        }

        #region IScreenshot Members

        public int Width { get; private set; }
        public int Height { get; private set; }

        public byte[] GetBuffer()
        {
            try
            {
                return GetBitmap();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }
}
