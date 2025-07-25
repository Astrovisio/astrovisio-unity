using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class ScreenRecorder : MonoBehaviour
{
    public int fps = 30;
    public float duration = 5f;
    public string outputVideoFileName = "output.mp4";

    private string framesFolder;
    private ConcurrentQueue<(string path, byte[] data)> saveQueue = new ConcurrentQueue<(string, byte[])>();
    private CancellationTokenSource cts;
    private Task saveTask;

    private void Start()
    {
        StartRecording();
    }

    public void StartRecording()
    {
        framesFolder = Path.Combine(Application.persistentDataPath, "frames");
        if (!Directory.Exists(framesFolder))
            Directory.CreateDirectory(framesFolder);

        cts = new CancellationTokenSource();
        saveTask = Task.Run(() => SaveWorker(cts.Token), cts.Token);

        StartCoroutine(CaptureFramesCoroutine());
    }

    private IEnumerator CaptureFramesCoroutine()
    {
        int frameCount = Mathf.CeilToInt(duration * fps);

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
            Texture2D frameTex = ScreenCapture.CaptureScreenshotAsTexture();

            byte[] imgData = frameTex.EncodeToJPG(50);
            string filePath = Path.Combine(framesFolder, $"frame_{i:D05}.jpg");
            saveQueue.Enqueue((filePath, imgData));

            Destroy(frameTex);
        }

        UnityEngine.Debug.Log("Frame capture completed. Waiting for images to be saved...");
        while (!saveQueue.IsEmpty)
        {
            yield return null;
        }

        cts.Cancel();
        saveTask.Wait();
        UnityEngine.Debug.Log("All frames saved. Starting ffmpeg...");
        CombineFramesToVideo();
    }

    // Worker thread for saving images to disk
    private void SaveWorker(CancellationToken token)
    {
        while (!token.IsCancellationRequested || !saveQueue.IsEmpty)
        {
            if (saveQueue.TryDequeue(out var item))
            {
                try
                {
                    File.WriteAllBytes(item.path, item.data);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning("Error while saving a frame: " + ex.Message);
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void CombineFramesToVideo()
    {
        string ffmpegPath = GetFFmpegPath();

        if (!File.Exists(ffmpegPath))
        {
            UnityEngine.Debug.LogError("ffmpeg.exe not found in: " + ffmpegPath);
            return;
        }

        string outputPath = Path.Combine(Application.persistentDataPath, outputVideoFileName);
        string framePattern = Path.Combine(framesFolder, "frame_%05d.jpg");

        // Ensure even dimensions to avoid libx264 errors
        string arguments = $"-r {fps} -i \"{framePattern}\" -vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" -c:v libx264 -pix_fmt yuv420p -y \"{outputPath}\"";

        UnityEngine.Debug.Log($"ffmpeg path: {ffmpegPath}");
        UnityEngine.Debug.Log($"Arguments: {arguments}");

        ProcessStartInfo proc = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process process = new Process { StartInfo = proc })
        {
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.Log(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.LogWarning(args.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        UnityEngine.Debug.Log("Video exported to: " + outputPath);

        // Remove temporary frames after video export
        try
        {
            Directory.Delete(framesFolder, true);
            UnityEngine.Debug.Log("Temporary frames deleted.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning("Error deleting temporary frames: " + ex.Message);
        }
    }

    private string GetFFmpegPath()
    {
        return Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
    }

    private void OnDestroy()
    {
        if (cts != null)
        {
            cts.Cancel();
        }
    }
    
}
