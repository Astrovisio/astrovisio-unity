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
        public static async Task<string> TakeScreenshot(
            Camera camera = null,
            bool transparentBackground = false,
            Dictionary<string, string> metadata = null,
            int timeoutSeconds = 10)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"screenshot_{timestamp}.png";
            string fullPath = Path.Combine(folderPath, filename);

            if (camera == null)
            {
                ScreenCapture.CaptureScreenshot(fullPath);
                Debug.Log($"Screenshot saved to: {fullPath}");
                await WaitForFile(fullPath, timeoutSeconds);
            }
            else
            {
                int width = Screen.width;
                int height = Screen.height;

                // Save camera settings
                var prevClearFlags = camera.clearFlags;
                var prevBgColor = camera.backgroundColor;

                RenderTexture prevRT = camera.targetTexture;

                RenderTexture rt;
                Texture2D tex;

                if (transparentBackground)
                {
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0, 0, 0, 0);

                    rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                    tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                }
                else
                {
                    rt = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
                    tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                }

                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                System.IO.File.WriteAllBytes(fullPath, bytes);

                // Reset camera settings
                camera.targetTexture = prevRT;
                camera.clearFlags = prevClearFlags;
                camera.backgroundColor = prevBgColor;
                RenderTexture.active = null;
                UnityEngine.Object.Destroy(rt);
                UnityEngine.Object.Destroy(tex);

                Debug.Log($"Camera screenshot saved to: {fullPath} (transparent: {transparentBackground})");
            }

            if (metadata != null)
            {
                AddMetadataToPNG(fullPath, metadata);
                Debug.Log("Metadata written to PNG: " + fullPath);
            }

            return fullPath;
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
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        using (FileStream fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                using (FileStream input = System.IO.File.OpenRead(filePath))
                using (FileStream output = System.IO.File.OpenWrite(tempPath))
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
                System.IO.File.Delete(filePath);
                System.IO.File.Move(tempPath, filePath);
                // Debug.Log("Final PNG file updated!");
            }
            catch// (Exception ex)
            {
                // Debug.LogError("Error in AddPngMetadata: " + ex);
                if (System.IO.File.Exists(tempPath))
                {
                    try { System.IO.File.Delete(tempPath); } catch { }
                }
            }
        }

    }

}
