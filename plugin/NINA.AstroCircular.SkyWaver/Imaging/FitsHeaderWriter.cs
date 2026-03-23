using System.Collections.Generic;

namespace NINA.AstroCircular.SkyWaver.Imaging {

    /// <summary>
    /// Ensures output FITS files have all headers required by SkyWave:
    /// FOCALLEN, XPIXSZ, YPIXSZ, XBINNING, YBINNING, EXPTIME, CCD-TEMP, FILTER.
    /// Pixel size is physical * binning (camera-side binning only).
    /// </summary>
    public static class FitsHeaderWriter {

        /// <summary>
        /// Build the FITS keyword dictionary for an integrated SKW collimation frame.
        /// </summary>
        /// <param name="focalLengthMm">Telescope focal length in mm</param>
        /// <param name="pixelSizeUm">Physical pixel size in microns</param>
        /// <param name="binning">Camera-side binning (1, 2, 3, or 4)</param>
        /// <param name="exposureSeconds">Total or average exposure time</param>
        /// <param name="ccdTemp">Sensor temperature in Celsius</param>
        /// <param name="filterName">Filter name (e.g. "R")</param>
        /// <returns>Dictionary of FITS keyword name → value pairs</returns>
        public static Dictionary<string, object> BuildHeaders(
            double focalLengthMm,
            double pixelSizeUm,
            int binning,
            double exposureSeconds,
            double ccdTemp,
            string filterName) {

            // XPIXSZ = physical (unbinned) pixel size — SkyWave uses XBINNING to compute effective size
            return new Dictionary<string, object> {
                { "FOCALLEN", focalLengthMm },
                { "XPIXSZ",  pixelSizeUm },
                { "YPIXSZ",  pixelSizeUm },
                { "XBINNING", binning },
                { "YBINNING", binning },
                { "EXPTIME",  exposureSeconds },
                { "CCD-TEMP", ccdTemp },
                { "FILTER",   filterName },
                { "COMMENT",  "Collimation Helper for SkyWave — Integrated Frame" }
            };
        }
    }
}
