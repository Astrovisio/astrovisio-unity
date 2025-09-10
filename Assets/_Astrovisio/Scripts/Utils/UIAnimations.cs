using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Astrovisio
{
    public static class UIAnimations
    {

        public static void SpinForever(this VisualElement ve, float secondsPerTurn = 3f)
        {
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ve.schedule.Execute(() =>
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                float t = (now - start) / 1000f;
                float angle = (t / secondsPerTurn) * 360f;
                ve.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            }).Every(16);
        }

        public static void PulseForever(this VisualElement ve, float durationSec = 2f)
        {
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ve.schedule.Execute(() =>
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                float t = ((now - start) / 1000f) % durationSec;
                float phase = t / durationSec;

                float s = 1f + 0.2f * (0.5f - 0.5f * Mathf.Cos(phase * Mathf.PI * 2f));
                ve.style.scale = new Scale(new Vector2(s, s));
            }).Every(16);
        }

        public static void ColorPulseForever(this VisualElement ve, Color a, Color b, float durationSec = 2f)
        {
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ve.schedule.Execute(() =>
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                float t = ((now - start) / 1000f) % durationSec;
                float phase = t / durationSec;

                // valore sinusoidale 0..1
                float lerp = 0.5f - 0.5f * Mathf.Cos(phase * Mathf.PI * 2f);
                ve.style.backgroundColor = Color.Lerp(a, b, lerp);
            }).Every(16);
        }


    }

}
