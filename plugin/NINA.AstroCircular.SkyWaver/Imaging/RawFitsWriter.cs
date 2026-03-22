using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NINA.AstroCircular.SkyWaver.Imaging {

    /// <summary>
    /// Writes a minimal valid 16-bit unsigned integer FITS file.
    /// Used for the integrated collimation output — avoids dependency on NINA's
    /// image pipeline for creating data from scratch (not from a capture).
    /// </summary>
    public static class RawFitsWriter {

        /// <summary>
        /// Write a 16-bit FITS file with the given pixel data and headers.
        /// FITS standard: 2880-byte blocks, ASCII header cards, big-endian data.
        /// </summary>
        /// <param name="filePath">Output file path</param>
        /// <param name="pixelData">16-bit pixel data (row-major)</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="extraHeaders">Additional FITS keyword/value pairs</param>
        public static void Write(string filePath, ushort[] pixelData, int width, int height,
            Dictionary<string, object> extraHeaders = null) {

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream)) {

                // Build header cards
                var cards = new List<string>();
                cards.Add(FormatCard("SIMPLE", "T", "conforms to FITS standard"));
                cards.Add(FormatCard("BITPIX", "16", "16-bit unsigned integer"));
                cards.Add(FormatCard("NAXIS", "2", "2D image"));
                cards.Add(FormatCard("NAXIS1", width.ToString(), "width in pixels"));
                cards.Add(FormatCard("NAXIS2", height.ToString(), "height in pixels"));
                cards.Add(FormatCard("BZERO", "32768", "unsigned 16-bit offset"));
                cards.Add(FormatCard("BSCALE", "1", "default scaling"));

                // Add extra headers (FOCALLEN, XPIXSZ, etc.)
                if (extraHeaders != null) {
                    foreach (var kvp in extraHeaders) {
                        if (kvp.Key == "COMMENT") {
                            cards.Add(FormatCommentCard(kvp.Value.ToString()));
                        } else if (kvp.Value is double d) {
                            cards.Add(FormatCard(kvp.Key, d.ToString("G"), ""));
                        } else if (kvp.Value is int i) {
                            cards.Add(FormatCard(kvp.Key, i.ToString(), ""));
                        } else {
                            cards.Add(FormatStringCard(kvp.Key, kvp.Value.ToString(), ""));
                        }
                    }
                }

                cards.Add("END".PadRight(80));

                // Write header — pad to 2880-byte boundary
                byte[] headerBytes = Encoding.ASCII.GetBytes(string.Join("", cards));
                writer.Write(headerBytes);
                int headerPad = (2880 - (headerBytes.Length % 2880)) % 2880;
                if (headerPad > 0) {
                    writer.Write(new byte[headerPad]); // zeros = spaces in ASCII, acceptable padding
                }

                // Write pixel data — FITS uses big-endian, with BZERO=32768 for unsigned 16-bit
                // Physical value = stored value * BSCALE + BZERO
                // So stored value = physical value - 32768 (stored as signed short)
                for (int p = 0; p < pixelData.Length; p++) {
                    short stored = (short)(pixelData[p] - 32768);
                    // Big-endian: high byte first
                    writer.Write((byte)((stored >> 8) & 0xFF));
                    writer.Write((byte)(stored & 0xFF));
                }

                // Pad data to 2880-byte boundary
                int dataBytes = pixelData.Length * 2;
                int dataPad = (2880 - (dataBytes % 2880)) % 2880;
                if (dataPad > 0) {
                    writer.Write(new byte[dataPad]);
                }
            }
        }

        /// <summary>Format a FITS header card: keyword = value / comment (80 chars)</summary>
        private static string FormatCard(string keyword, string value, string comment) {
            string card = $"{keyword.PadRight(8)}= {value.PadLeft(20)}";
            if (!string.IsNullOrEmpty(comment)) {
                card += $" / {comment}";
            }
            return card.PadRight(80).Substring(0, 80);
        }

        /// <summary>Format a FITS string value card: keyword = 'value' / comment</summary>
        private static string FormatStringCard(string keyword, string value, string comment) {
            string quotedValue = $"'{value}'";
            string card = $"{keyword.PadRight(8)}= {quotedValue.PadRight(20)}";
            if (!string.IsNullOrEmpty(comment)) {
                card += $" / {comment}";
            }
            return card.PadRight(80).Substring(0, 80);
        }

        /// <summary>Format a FITS COMMENT card</summary>
        private static string FormatCommentCard(string text) {
            return $"COMMENT {text}".PadRight(80).Substring(0, 80);
        }
    }
}
