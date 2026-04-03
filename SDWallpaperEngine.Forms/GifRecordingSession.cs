using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace SDWallpaperEngine.Forms
{
    internal sealed class GifRecordingSession : IDisposable
    {
        private readonly string _recordingsDirectory;
        private readonly string _framesDirectory;
        private readonly List<GifFrame> _frames = [];

        private GifRecordingSession(string recordingsDirectory, string framesDirectory, string outputFilePath)
        {
            _recordingsDirectory = recordingsDirectory;
            _framesDirectory = framesDirectory;
            OutputFilePath = outputFilePath;
        }

        public string OutputFilePath { get; }

        public static GifRecordingSession Start(string outputDirectory)
        {
            var recordingsDirectory = Path.Combine(outputDirectory, "Recordings");
            Directory.CreateDirectory(recordingsDirectory);

            var sessionName = $"record_{DateTime.Now:yyyyMMdd_HHmmss}";
            var framesDirectory = Path.Combine(recordingsDirectory, $"{sessionName}_frames");
            Directory.CreateDirectory(framesDirectory);

            var outputFilePath = Path.Combine(recordingsDirectory, $"{sessionName}.gif");
            return new GifRecordingSession(recordingsDirectory, framesDirectory, outputFilePath);
        }

        public void AddFrame(string imagePath, int delayMs)
        {
            if (!File.Exists(imagePath))
            {
                return;
            }

            var targetPath = Path.Combine(_framesDirectory, $"frame_{_frames.Count:000000}.bmp");
            File.Copy(imagePath, targetPath, overwrite: true);
            _frames.Add(new GifFrame(targetPath, Math.Max(10, delayMs)));
        }

        public string? FinalizeAndSave()
        {
            if (_frames.Count < 2)
            {
                return null;
            }

            var gifEncoder = GetGifEncoder();
            if (gifEncoder is null)
            {
                throw new InvalidOperationException("GIF encoder not found.");
            }

            using var firstFrame = Image.FromFile(_frames[0].Path);
            SetAnimationProperties(firstFrame, _frames.Select(frame => frame.DelayMs).ToArray());

            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            firstFrame.Save(OutputFilePath, gifEncoder, encoderParameters);

            for (var index = 1; index < _frames.Count; index++)
            {
                using var frameImage = Image.FromFile(_frames[index].Path);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionTime);
                firstFrame.SaveAdd(frameImage, encoderParameters);
            }

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            firstFrame.SaveAdd(encoderParameters);

            return OutputFilePath;
        }

        private static void SetAnimationProperties(Image image, IReadOnlyList<int> delaysMs)
        {
            var delayProperty = CreatePropertyItem();
            delayProperty.Id = 0x5100;
            delayProperty.Type = 4;
            delayProperty.Len = delaysMs.Count * 4;
            delayProperty.Value = new byte[delayProperty.Len];

            for (var i = 0; i < delaysMs.Count; i++)
            {
                var frameDelayHundredths = Math.Max(1, delaysMs[i] / 10);
                var bytes = BitConverter.GetBytes(frameDelayHundredths);
                Buffer.BlockCopy(bytes, 0, delayProperty.Value, i * 4, 4);
            }

            image.SetPropertyItem(delayProperty);

            var loopProperty = CreatePropertyItem();
            loopProperty.Id = 0x5101;
            loopProperty.Type = 3;
            loopProperty.Len = 2;
            loopProperty.Value = BitConverter.GetBytes((ushort)0);
            image.SetPropertyItem(loopProperty);
        }

        private static PropertyItem CreatePropertyItem()
        {
            return (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        }

        private static ImageCodecInfo? GetGifEncoder()
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Gif.Guid);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_framesDirectory))
                {
                    Directory.Delete(_framesDirectory, recursive: true);
                }
            }
            catch
            {
            }
        }

        private sealed record GifFrame(string Path, int DelayMs);
    }
}
