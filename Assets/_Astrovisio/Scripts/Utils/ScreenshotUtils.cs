using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hjg.Pngcs;
using UnityEngine;

namespace Astrovisio
{
    public static class ScreenshotUtils
    {

        /// <summary>
        /// Takes a screenshot and writes metadata as soon as the file is ready.
        /// Fully async, does NOT block the main thread.
        /// </summary>
        public static async Task TakeScreenshot(Dictionary<string, string> metadata = null, int timeoutSeconds = 10)
        {
            string folderPath = Application.persistentDataPath + "/Screenshots";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                // Debug.Log($"Created screenshot directory at: {folderPath}");
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string screenshotFilename = $"screenshot_{timestamp}.png";
            string fullPath = Path.Combine(folderPath, screenshotFilename);

            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"Screenshot saved to: {fullPath}");

            // Wait asynchronously for the file to be ready
            await WaitForFile(fullPath, timeoutSeconds);

            if (metadata == null)
            {
                return;
            }

            AddMetadataToPNG(fullPath, metadata);
            // Debug.Log("Written to PNG: " + fullPath);
        }

        /// <summary>
        /// Asynchronously waits until the file exists and is accessible for reading.
        /// Does not block the main thread!
        /// </summary>
        private static async Task WaitForFile(string filePath, int timeoutSeconds = 10)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            if (fs.Length > 0)
                                return;
                        }
                    }
                    catch { /* File is still being written */ }
                }
                await Task.Delay(100);
            }
            // Debug.LogWarning($"Timeout waiting for file: {filePath}");
        }

        /// <summary>
        /// Injects or updates textual metadata in a PNG file.
        /// </summary>
        public static void AddMetadataToPNG(string filePath, Dictionary<string, string> metadata)
        {
            string tempPath = filePath + ".tmp";
            // Debug.Log("AddPngMetadata > filePath: " + filePath);
            // Debug.Log("AddPngMetadata > tempPath: " + tempPath);

            try
            {
                using (FileStream input = File.OpenRead(filePath))
                using (FileStream output = File.OpenWrite(tempPath))
                {
                    // Debug.Log("Opening PngReader...");
                    PngReader pngr = new PngReader(input);
                    // Debug.Log("PngReader opened!");

                    ImageInfo info = pngr.ImgInfo;
                    // Debug.Log($"ImageInfo: rows={info.Rows}, cols={info.Cols}");

                    // Debug.Log("Reading all rows with ReadRowsInt...");
                    ImageLines imdata = pngr.ReadRowsInt();

                    // Debug.Log($"imdata null? {imdata == null}");
                    // Debug.Log($"imdata.Nrows: {imdata.Nrows}");
                    // Debug.Log($"imdata.ImgInfo: {imdata.ImgInfo}");

                    pngr.End();

                    // Debug.Log("Creating PngWriter...");
                    PngWriter pngw = new PngWriter(output, info);

                    // Write all metadata to the new PNG file
                    foreach (var kv in metadata)
                    {
                        pngw.GetMetadata().SetText(kv.Key, kv.Value);
                        Debug.Log("Added metadata: " + kv.Key + "=" + kv.Value);
                    }

                    for (int row = 0; row < info.Rows; row++)
                    {
                        int[] imgline = imdata.Scanlines[row];
                        pngw.WriteRow(imgline, row);
                    }
                    pngw.End();
                    // Debug.Log("PngWriter end!");
                }

                // Replace the original file
                // Debug.Log("Deleting the original file and renaming the temp file...");
                File.Delete(filePath);
                File.Move(tempPath, filePath);
                // Debug.Log("Final PNG file updated!");
            }
            catch// (Exception ex)
            {
                // Debug.LogError("Error in AddPngMetadata: " + ex);
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

    }
    
}
