using NINA.Image.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.AstroCircular.SkyWaver.Imaging {

    /// <summary>
    /// Native FITS frame integration: pixel-by-pixel averaging, optional crop-to-circle,
    /// optional bin 2x2 downsample. Produces a 16-bit monochrome array for SkyWave.
    /// </summary>
    public class FitsAverager {

        /// <summary>
        /// Result of the integration process.
        /// </summary>
        public class IntegrationResult {
            public ushort[] PixelData { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int FrameCount { get; set; }
        }

        /// <summary>
        /// Load FITS files and compute pixel-by-pixel average.
        /// </summary>
        /// <param name="inputFiles">Paths to FITS sub-frames</param>
        /// <param name="imageDataFactory">NINA's image data factory for reading FITS</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Averaged pixel data as double array with dimensions</returns>
        public static async Task<(double[] data, int width, int height, int frameCount)> Average(
            List<string> inputFiles, IImageDataFactory imageDataFactory, CancellationToken ct) {

            if (inputFiles == null || inputFiles.Count < 2) {
                throw new InvalidOperationException("Need at least 2 frames for integration.");
            }

            // Load first frame to get dimensions
            var firstImage = await imageDataFactory.CreateFromFile(inputFiles[0], 16, false, NINA.Core.Enum.RawConverterEnum.FREEIMAGE, ct);
            int width = firstImage.Properties.Width;
            int height = firstImage.Properties.Height;
            int pixelCount = width * height;

            // Accumulator buffer
            double[] accumulator = new double[pixelCount];
            var firstData = firstImage.Data.FlatArray;
            for (int p = 0; p < pixelCount; p++) {
                accumulator[p] = firstData[p];
            }

            // Add remaining frames
            int frameCount = 1;
            for (int f = 1; f < inputFiles.Count; f++) {
                ct.ThrowIfCancellationRequested();

                if (!File.Exists(inputFiles[f])) continue;

                var image = await imageDataFactory.CreateFromFile(inputFiles[f], 16, false, NINA.Core.Enum.RawConverterEnum.FREEIMAGE, ct);
                if (image.Properties.Width != width || image.Properties.Height != height) {
                    // Skip frames with mismatched dimensions
                    continue;
                }

                var data = image.Data.FlatArray;
                for (int p = 0; p < pixelCount; p++) {
                    accumulator[p] += data[p];
                }
                frameCount++;
            }

            // Divide by frame count to get average
            for (int p = 0; p < pixelCount; p++) {
                accumulator[p] /= frameCount;
            }

            return (accumulator, width, height, frameCount);
        }

        /// <summary>
        /// Crop the image to a centered square (shorter dimension).
        /// Rectangular crop — no circular masking.
        /// </summary>
        public static (double[] data, int newWidth, int newHeight) CropToSquare(
            double[] data, int width, int height) {

            int cropSize = Math.Min(width, height);
            int cropX = (width - cropSize) / 2;
            int cropY = (height - cropSize) / 2;

            double[] cropped = new double[cropSize * cropSize];

            for (int y = 0; y < cropSize; y++) {
                for (int x = 0; x < cropSize; x++) {
                    int srcX = cropX + x;
                    int srcY = cropY + y;
                    if (srcX >= 0 && srcX < width && srcY >= 0 && srcY < height) {
                        cropped[y * cropSize + x] = data[srcY * width + srcX];
                    }
                }
            }

            return (cropped, cropSize, cropSize);
        }

        /// <summary>
        /// Bin 2x2: average each 2x2 pixel block into a single pixel.
        /// Output is half the dimensions of the input.
        /// </summary>
        public static (double[] data, int newWidth, int newHeight) Bin2x2(
            double[] data, int width, int height) {

            int newWidth = width / 2;
            int newHeight = height / 2;
            double[] binned = new double[newWidth * newHeight];

            for (int by = 0; by < newHeight; by++) {
                for (int bx = 0; bx < newWidth; bx++) {
                    int sx = bx * 2;
                    int sy = by * 2;
                    double sum = data[sy * width + sx]
                               + data[sy * width + sx + 1]
                               + data[(sy + 1) * width + sx]
                               + data[(sy + 1) * width + sx + 1];
                    binned[by * newWidth + bx] = sum / 4.0;
                }
            }

            return (binned, newWidth, newHeight);
        }

        /// <summary>
        /// Convert double array to 16-bit unsigned integer array, clamping to valid range.
        /// </summary>
        public static ushort[] ToUShort16(double[] data) {
            ushort[] result = new ushort[data.Length];
            for (int i = 0; i < data.Length; i++) {
                result[i] = (ushort)Math.Max(0, Math.Min(65535, Math.Round(data[i])));
            }
            return result;
        }
    }
}
