using System;
using System.IO;
using System.Linq;
using System.Threading;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using static Lennox.NvEncSharp.LibNvEnc;
using Xabe.FFmpeg;
using System.Windows;
using System.Diagnostics;
using System.Timers;
using Lennox.NvEncSharp;
using System.Threading.Tasks;
using SharpDX;

namespace MuhAimLabScoresViewer
{
    internal class ScreenCaptureNvenc
    {
        private bool _initialized = false;
        private NvEncoder _encoder;
        private NvEncCreateBitstreamBuffer _bitstreamBuffer;
        private readonly object _writeMutex = new object();

        private const int framerate = 60;
        private const int _fps = 1000 / 60;
        private const int _frameDuration = 2; //10;

        private static string outputPath;
        private static string outputFileName;

        private static int bitrate;

        private static MemoryStream memStream;
        private static BufferSecondsQueue bufferQueue;

        private static int replaySeconds;

        private Task recordingTask = null;

        public static bool _keepRecording = false;


        static SharpDX.Direct3D11.Device? device = null;
        static Output1? displayOutput = null;

        public ScreenCaptureNvenc(string[] args)
        {
            //if (args.Length < 1) args = new string[] { "-d", "\\\\.\\DISPLAY1", "-o", " D:/ballertest/", "-r", "24", "-f", "60", "-s", "30", "-file", "test" };

            if (!_keepRecording) _keepRecording = true;
            recordingTask = null;   

            var pArgs = new ProgramArguments(args);
            recordingTask = Task.Run(() =>
            {
                Run(pArgs);
            });
        }

        public void saveReplay()
        {
            Logger.log("saving replay...");
            Task.Factory.StartNew(saveH264).ContinueWith(task =>
            {
                convertToMP4(task.Result);
            });
        }

        public string saveH264()
        {
            string outputFilePath = Path.Combine(outputPath, outputFileName) + ".264";
            Logger.log(outputFilePath);
            File.WriteAllBytes(outputFilePath, bufferQueue.getFullBuffer());
            Logger.log("h264 file written!");

            return outputFilePath;
        }

        public void convertToMP4(string h264)
        {
            Logger.log("converting to mp4...");
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.FileName = @"C:\windows\system32\cmd.exe";
            string mp4 = h264.Substring(0, h264.Length - 3) + "mp4";
            proc.Arguments = $"/c D:\\ballertest\\ffmpeg-master\\bin\\ffmpeg.exe -y -i {h264} -c copy {mp4}"; //-loglevel quiet -nostats
            var p = Process.Start(proc);
            Logger.log("wrapped in mp4 container!");
        }

        public void stop()
        {
            _keepRecording = false;
            //recordingTask.Dispose();

            Logger.log("Cleanup complete");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("putting second into buffer!");
            bufferQueue.push(memStream.ToArray());
            memStream.SetLength(0);
        }

        private void Run(ProgramArguments args)
        {
            _keepRecording = true;
            //using var duplicate = GetDisplayDuplicate(args.DisplayName, out var outputDescription);
            memStream = new MemoryStream(); //using var fileoutput = File.Open(args.OutputPath, FileMode.Create);
            outputPath = args.OutputFolder;
            bitrate = args.Bitrate;
            replaySeconds = args.BufferSeconds;
            outputFileName = args.OutputFile;

            Console.WriteLine($"Process: {(Environment.Is64BitProcess ? "64" : "32")} bits");
            //Console.WriteLine($"Display: {outputDescription.DeviceName}");
            Console.WriteLine($"Output: {outputPath}");
            // MessageBox.Show(outputPath + outputFileName + ".264");
            //Console.WriteLine($"Bitrate: {bitrate} mbps");
            //Console.WriteLine($"FPS: {framerate}");

            bufferQueue = new BufferSecondsQueue(replaySeconds);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var duplicate = GetDisplayDuplicate(args.DisplayName, out var outputDescription);
            SharpDX.DXGI.Resource? resourceOut = null;
            Texture2D? desktopTexture = null;

            while (_keepRecording)
            {

                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    bufferQueue.push(memStream.ToArray());
                    memStream.SetLength(0);
                    stopwatch.Restart();
                }

                try
                {
                    var code = duplicate.TryAcquireNextFrame(-1, out var frameInfo, out resourceOut); // Get the next screen image.

                    if(code.Code < 0) //means error
                    {
                        
                        duplicate = GetDisplayDuplicate(args.DisplayName, out outputDescription);
                        resourceOut = null;
                        desktopTexture = null;

                        duplicate.TryAcquireNextFrame(-1, out frameInfo, out resourceOut);
                    }


                    if (frameInfo.LastPresentTime == 0) // If the frame has not changed, there's no reason to encode a new frame.
                    {
                        duplicate.ReleaseFrame();
                        resourceOut.Dispose();
                        Thread.Sleep(_frameDuration);
                        continue;
                    }

                    desktopTexture = resourceOut.QueryInterface<Texture2D>();
                    var encoder = _initialized ? _encoder : CreateEncoder(desktopTexture);
                    var desc = desktopTexture.Description;
                    desc.OptionFlags = ResourceOptionFlags.SharedKeyedmutex;

                    var reg = new NvEncRegisterResource
                    {
                        Version = NV_ENC_REGISTER_RESOURCE_VER,
                        BufferFormat = NvEncBufferFormat.Abgr,
                        BufferUsage = NvEncBufferUsage.NvEncInputImage,
                        ResourceToRegister = desktopTexture.NativePointer,
                        Width = (uint)desc.Width,
                        Height = (uint)desc.Height,
                        Pitch = 0
                    };

                    using var _ = _encoder.RegisterResource(ref reg); // Registers the hardware texture surface as a resource for NvEnc to use.

                    var pic = new NvEncPicParams
                    {
                        Version = NV_ENC_PIC_PARAMS_VER,
                        PictureStruct = NvEncPicStruct.Frame,
                        InputBuffer = reg.AsInputPointer(),
                        BufferFmt = NvEncBufferFormat.Abgr,
                        InputWidth = (uint)desc.Width,
                        InputHeight = (uint)desc.Height,
                        OutputBitstream = _bitstreamBuffer.BitstreamBuffer,
                        InputTimeStamp = (ulong)frameInfo.LastPresentTime,
                        InputDuration = _frameDuration
                    };
                    encoder.EncodePicture(ref pic); // Do the actual encoding. With this configuration this is done sync (blocking).

                    // The output is written to the bitstream, which is now copied to the output file.
                    using (var sm = encoder.LockBitstreamAndCreateStream(ref _bitstreamBuffer))
                    {
                        lock (_writeMutex)
                        {
                            sm.CopyTo(memStream);
                        }
                    }

                    desktopTexture.Dispose();
                    duplicate.ReleaseFrame();
                    resourceOut.Dispose();
                    Thread.Sleep(_frameDuration);
                }
                catch (Exception ex)
                {
                    Logger.log("ScreenCaptureNvenc Exception!" + Environment.NewLine + ex.Message);
                    
                    /*if (ex.ResultCode == SharpDX.DXGI.ResultCode.AccessLost) //
                    {
                        Logger.log("access lost error!");

                    }*/
                    // ReSharper disable once FunctionNeverReturns
                }
            }
        }

       static Output? output;

        private static OutputDuplication GetDisplayDuplicate(string displayName, out OutputDescription description)
        {
            using var factory = new Factory4();
            var availableAdaptors = factory.Adapters;

            output = availableAdaptors.SelectMany(t => t.Outputs).FirstOrDefault(t => displayName == null ?
                            t.Description.IsAttachedToDesktop == true : t.Description.DeviceName == displayName);

            if (output == null) throw new DriveNotFoundException(displayName);

            var foundDeviceName = output.Description.DeviceName;
            using var dxgiAdapter = output.GetParent<Adapter>();
            device = new SharpDX.Direct3D11.Device(dxgiAdapter);
           
            var dxgiOutput = dxgiAdapter.Outputs.Single(t => t.Description.DeviceName == foundDeviceName);

            displayOutput = dxgiOutput.QueryInterface<Output1>();
            description = displayOutput.Description;
           
            return displayOutput.DuplicateOutput(device); //"for improved performance consider using DuplicateOutput1()"
        }

        private NvEncoder CreateEncoder(Texture2D texture)
        {
            if (_initialized) return _encoder;

            var desc = texture.Description;
            var encoder = OpenEncoderForDirectX(texture.Device.NativePointer);
            var encoderConfig = encoder.GetEncodePresetConfig(NvEncCodecGuids.H264, NvEncPresetGuids.LowLatencyDefault).PresetCfg;
            encoderConfig.RcParams.AverageBitRate = 24 * (1 << 20); // 16 //4 Mbit (uint)bitrate
            encoderConfig.RcParams.MaxBitRate = 32 * (1 << 20);
            encoderConfig.RcParams.RateControlMode = NvEncParamsRcMode.Vbr;

            unsafe
            {
                NvEncConfig* p = &encoderConfig;
                var initparams = new NvEncInitializeParams
                {
                    Version = NV_ENC_INITIALIZE_PARAMS_VER,
                    EncodeGuid = NvEncCodecGuids.H264,
                    EncodeHeight = (uint)desc.Height,
                    EncodeWidth = (uint)desc.Width,
                    MaxEncodeHeight = (uint)desc.Height,
                    MaxEncodeWidth = (uint)desc.Width,
                    DarHeight = (uint)desc.Height,
                    DarWidth = (uint)desc.Width,
                    FrameRateNum = (uint)framerate,
                    FrameRateDen = 1,
                    ReportSliceOffsets = false,
                    EnableSubFrameWrite = false,
                    PresetGuid = NvEncPresetGuids.LowLatencyDefault,
                    EnableEncodeAsync = 0,
                    EnablePTD = 1,
                    EnableWeightedPrediction = true,
                    EncodeConfig = p,
                };
                encoder.InitializeEncoder(ref initparams);
            }
            _bitstreamBuffer = encoder.CreateBitstreamBuffer();
            _encoder = encoder;
            _initialized = true;
            return encoder;
        }
    }
}
