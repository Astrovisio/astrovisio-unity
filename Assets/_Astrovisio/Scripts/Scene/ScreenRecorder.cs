using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.IO;

public class ScreenRecorder : MonoBehaviour
{
    public int fps = 30;
    public float duration = 5f;
    public string outputVideoFileName = "output.mp4";

    private string framesFolder;

    private void Start()
    {
        StartRecording();
    }

    public void StartRecording()
    {
        framesFolder = Path.Combine(Application.persistentDataPath, "frames");
        if (!Directory.Exists(framesFolder))
        {
            Directory.CreateDirectory(framesFolder);
        }

        StartCoroutine(CaptureFramesCoroutine());
    }

    private IEnumerator CaptureFramesCoroutine()
    {
        int frameCount = Mathf.CeilToInt(duration * fps);
        float interval = 1.0f / fps;

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(framesFolder, $"frame_{i:D05}.png"), pngData);
            Destroy(tex);
            yield return new WaitForSecondsRealtime(interval);
        }
        UnityEngine.Debug.Log("Cattura frame completata. Avvio ffmpeg...");
        CombineFramesToVideo();
    }

    private void CombineFramesToVideo()
    {
        string ffmpegPath = GetFFmpegPath();

        if (!File.Exists(ffmpegPath))
        {
            UnityEngine.Debug.LogError("ffmpeg.exe non trovato in: " + ffmpegPath);
            return;
        }

        string outputPath = Path.Combine(Application.persistentDataPath, outputVideoFileName);
        string framePattern = Path.Combine(framesFolder, "frame_%05d.png");

        string arguments = $"-r {fps} -i \"{framePattern}\" -vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2\" -c:v libx264 -pix_fmt yuv420p -y \"{outputPath}\"";

        UnityEngine.Debug.LogError($"Percorso ffmpeg: {ffmpegPath}");
        UnityEngine.Debug.LogError($"Arguments: {arguments}");

        ProcessStartInfo proc = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process process = new Process { StartInfo = proc };
        process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
        process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogWarning(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        UnityEngine.Debug.Log("Video esportato su: " + outputPath);
    }

    private string GetFFmpegPath()
    {
        // Costruisce sempre il percorso assoluto a ffmpeg.exe in StreamingAssets
        string path = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
        return path;
    }

    private bool FFmpegExists()
    {
        return File.Exists(GetFFmpegPath());
    }

}
