using System.Collections.Generic;

namespace NINA.AstroCircular.SkyWaver.Imaging {

    /// <summary>
    /// Ensures output FITS files have all headers required by SkyWave:
    /// FOCALLEN, XPIXSZ, YPIXSZ, XBINNING, YBINNING, EXPTIME, CCD-TEMP, FILTER.
    /// Updates pixel size and binning headers when bin2 is applied.
    /// </summary>
    public static class FitsHeaderWriter {

        /// <summary>
        /// Build the FITS keyword dictionary for an integrated SKW collimation frame.
        /// </summary>
        /// <param name="focalLengthMm">Telescope focal length in mm</param>
        /// <param name="pixelSizeUm">Physical pixel size in microns</param>
        /// <param name="captureBinning">Binning used during capture (1 or 2)</param>
        /// <param name="wasBinned">True if post-integration bin2 was applied</param>
        /// <param name="exposureSeconds">Total or average exposure time</param>
        /// <param name="ccdTemp">Sensor temperature in Celsius</param>
        /// <param name="filterName">Filter name (e.g. "R")</param>
        /// <returns>Dictionary of FITS keyword name → value pairs</returns>
        public static Dictionary<string, object> BuildHeaders(
            double focalLengthMm,
            double pixelSizeUm,
            int captureBinning,
            bool wasBinned,
            double exposureSeconds,
            double ccdTemp,
            string filterName) {

            // Effective pixel size: physical * capture binning * (2 if post-bin applied)
            double effectivePixelSize = pixelSizeUm * captureBinning * (wasBinned ? 2.0 : 1.0);
            int effectiveBinning = captureBinning * (wasBinned ? 2 : 1);

            return new Dictionary<string, object> {
                { "FOCALLEN", focalLengthMm },
                { "XPIXSZ",  effectivePixelSize },
                { "YPIXSZ",  effectivePixelSize },
                { "XBINNING", effectiveBinning },
                { "YBINNING", effectiveBinning },
                { "EXPTIME",  exposureSeconds },
                { "CCD-TEMP", ccdTemp },
                { "FILTER",   filterName },
                { "COMMENT",  "Collimation Helper for SkyWave — Integrated Frame" }
            };
        }
    }
}
