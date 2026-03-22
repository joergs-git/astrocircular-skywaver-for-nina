using Newtonsoft.Json;
using NINA.AstroCircular.SkyWaver.Imaging;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Image.Interfaces;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.AstroCircular.SkyWaver.SequenceItems {

    /// <summary>
    /// Integrates SKW collimation sub-frames: pixel-average, optional crop, optional bin2,
    /// write 16-bit FITS with proper headers to SkyWave output directory.
    /// </summary>
    [ExportMetadata("Name", "SKW Integrate Frames")]
    [ExportMetadata("Description", "Average sub-frames and produce SkyWave-ready FITS output")]
    [ExportMetadata("Icon", "SettingsSVG")]
    [ExportMetadata("Category", "AstroCircular SKW")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkwIntegrateFrames : SequenceItem {
        private readonly IImageDataFactory imageDataFactory;

        [JsonProperty]
        public string InputDirectory { get; set; } = "";

        [JsonProperty]
        public string OutputDirectory { get; set; } = "";

        [JsonProperty]
        public string OutputFileName { get; set; } = "SKW_Collimation_Integrated.fits";

        [JsonProperty]
        public bool CropToCircle { get; set; } = true;

        [JsonProperty]
        public bool BinToHalf { get; set; } = true;

        // Sensor/telescope params for FITS header updates
        [JsonProperty]
        public double FocalLengthMm { get; set; } = 1946;

        [JsonProperty]
        public double PixelSizeUm { get; set; } = 3.76;

        [JsonProperty]
        public int CaptureBinning { get; set; } = 1;

        [JsonProperty]
        public double ExposureSeconds { get; set; } = 8;

        [JsonProperty]
        public string FilterName { get; set; } = "R";

        [JsonProperty]
        public bool AutoCleanSubFrames { get; set; } = true;

        /// <summary>
        /// Alternatively, pass file paths directly instead of scanning a directory.
        /// Set by SkwCollimationRun orchestrator.
        /// </summary>
        public List<string> InputFiles { get; set; }

        [ImportingConstructor]
        public SkwIntegrateFrames(IImageDataFactory imageDataFactory) {
            this.imageDataFactory = imageDataFactory;
        }

        private SkwIntegrateFrames(SkwIntegrateFrames cloneMe) : this(cloneMe.imageDataFactory) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SkwIntegrateFrames(this) {
                InputDirectory = InputDirectory,
                OutputDirectory = OutputDirectory,
                OutputFileName = OutputFileName,
                CropToCircle = CropToCircle,
                BinToHalf = BinToHalf,
                FocalLengthMm = FocalLengthMm,
                PixelSizeUm = PixelSizeUm,
                CaptureBinning = CaptureBinning,
                ExposureSeconds = ExposureSeconds,
                FilterName = FilterName,
                AutoCleanSubFrames = AutoCleanSubFrames
            };
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Gather input files
            List<string> files;
            if (InputFiles != null && InputFiles.Count > 0) {
                files = InputFiles.Where(File.Exists).ToList();
            } else if (!string.IsNullOrEmpty(InputDirectory) && Directory.Exists(InputDirectory)) {
                files = Directory.GetFiles(InputDirectory, "*.fits")
                    .Concat(Directory.GetFiles(InputDirectory, "*.fit"))
                    .Concat(Directory.GetFiles(InputDirectory, "*.fts"))
                    .OrderBy(f => f)
                    .ToList();
            } else {
                throw new SequenceEntityFailedException("SKW Integration: No input files or directory specified.");
            }

            if (files.Count < 2) {
                throw new SequenceEntityFailedException($"SKW Integration: Found only {files.Count} FITS files. Need at least 2.");
            }

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Integrating {files.Count} frames..."
            });

            // Step 1: Average all frames
            var (averaged, width, height, frameCount) = await FitsAverager.Average(files, imageDataFactory, ct);

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Averaged {frameCount} frames ({width}x{height})"
            });

            // Step 2: Optional crop to inscribed circle
            if (CropToCircle) {
                (averaged, width, height) = FitsAverager.CropToSquare(averaged, width, height);
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Cropped to circle ({width}x{height})"
                });
            }

            // Step 3: Optional bin 2x2
            if (BinToHalf) {
                (averaged, width, height) = FitsAverager.Bin2x2(averaged, width, height);
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Binned 2x ({width}x{height})"
                });
            }

            // Step 4: Convert to 16-bit
            ushort[] pixelData = FitsAverager.ToUShort16(averaged);

            // Step 5: Build FITS headers with correct values
            var headers = FitsHeaderWriter.BuildHeaders(
                FocalLengthMm, PixelSizeUm, CaptureBinning, BinToHalf,
                ExposureSeconds, -999, FilterName); // -999 placeholder for CCD temp

            // Step 6: Save output FITS using raw FITS writer
            // We write a minimal valid FITS file directly rather than going through
            // NINA's image pipeline, since we're creating data from scratch (not a capture).
            string outputDir = !string.IsNullOrEmpty(OutputDirectory) ? OutputDirectory : InputDirectory;
            Directory.CreateDirectory(outputDir);
            string outputPath = Path.Combine(outputDir, OutputFileName);

            RawFitsWriter.Write(outputPath, pixelData, width, height, headers);

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Saved integrated FITS to {outputPath}"
            });

            // Step 7: Optional cleanup of sub-frames
            if (AutoCleanSubFrames && !string.IsNullOrEmpty(InputDirectory)) {
                try {
                    foreach (var file in files) {
                        if (File.Exists(file)) File.Delete(file);
                    }
                    // Remove temp directory if empty
                    if (Directory.Exists(InputDirectory) && !Directory.EnumerateFileSystemEntries(InputDirectory).Any()) {
                        Directory.Delete(InputDirectory);
                    }
                    progress?.Report(new ApplicationStatus {
                        Status = "SKW: Cleaned up sub-frames"
                    });
                } catch {
                    // Non-critical — log but don't fail
                    Logger.Warning("SKW: Could not clean up some sub-frame files.");
                }
            }
        }

        public override string ToString() {
            return $"Category: AstroCircular SKW, Item: SkwIntegrateFrames, Crop: {CropToCircle}, Bin2: {BinToHalf}";
        }
    }
}
