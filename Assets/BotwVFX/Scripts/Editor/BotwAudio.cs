using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Sonidos provisionales sintetizados por código (WAV en Assets/BotwVFX/Audio).
    /// No pretenden sonar como el juego, solo demostrar cuánto aporta el audio
    /// al "peso" de un efecto. Sustitúyelos por sonidos de verdad cuando los tengas.
    /// </summary>
    public static class BotwAudio
    {
        public const string Folder = "Assets/BotwVFX/Audio";
        const int Rate = 44100;

        public static AudioClip Load(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{Folder}/{name}.wav");

        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            Save("SFX_RemoteBomb", RemoteBomb());
            Save("SFX_Explosion", Explosion());
            Save("SFX_GuardianBeeps", GuardianBeeps());
            Save("SFX_GuardianCharge", GuardianCharge());
            Save("SFX_GuardianShot", GuardianShot());
            Save("SFX_ArrowWhoosh", ArrowWhoosh());
            Save("SFX_ArrowPortal", ArrowPortal());
        }

        // ------------------------------------------------------------ sonidos

        static float[] RemoteBomb()
        {
            var s = Buffer(1.4f);
            var rng = new System.Random(1);
            float lp = 0f, phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float noise = Noise(rng);
                float crack = noise * Mathf.Exp(-t / 0.015f);
                phase += 2f * Mathf.PI * (40f + 70f * Mathf.Exp(-t * 3f)) / Rate;
                float boom = Mathf.Sin(phase) * Mathf.Exp(-t / 0.35f);
                lp = LowPass(lp, noise, 300f);
                float rumble = lp * 3f * Mathf.Exp(-t / 0.45f);
                // Brillo "Sheikah": acorde agudo que se apaga.
                float shimmer = (Mathf.Sin(2f * Mathf.PI * 1318f * t) + Mathf.Sin(2f * Mathf.PI * 1975f * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * 2637f * t) * 0.5f)
                                * 0.12f * Mathf.Exp(-t / 0.5f) * (0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 9f * t));
                s[i] = crack * 0.8f + boom + rumble + shimmer;
            }
            return s;
        }

        static float[] Explosion()
        {
            var s = Buffer(2.6f);
            var rng = new System.Random(2);
            float lp = 0f, lp2 = 0f, phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float noise = Noise(rng);
                float crack = noise * Mathf.Exp(-t / 0.02f);
                phase += 2f * Mathf.PI * (32f + 55f * Mathf.Exp(-t * 2.5f)) / Rate;
                float boom = Mathf.Sin(phase) * Mathf.Exp(-t / 0.6f) * 1.2f;
                lp = LowPass(lp, noise, 220f);
                lp2 = LowPass(lp2, noise, 1200f);
                float rumble = lp * 4f * Mathf.Exp(-t / 0.9f) + lp2 * 0.5f * Mathf.Exp(-t / 0.25f);
                // Chisporroteo: clics aleatorios cada vez más escasos.
                float crackle = rng.NextDouble() < 0.004 * Mathf.Exp(-t / 0.6f) ? Noise(rng) * 2f : 0f;
                s[i] = crack + boom + rumble + crackle;
            }
            return s;
        }

        // Pitidos que aceleran y terminan en un tono continuo antes del disparo.
        static float[] GuardianBeeps()
        {
            const float length = 2.7f;
            var s = Buffer(length);
            float interval = 0.42f, next = 0.05f;
            while (next < length - 0.4f)
            {
                AddBeep(s, next, 0.06f, 1046f, 0.5f);
                next += interval;
                interval = Mathf.Max(0.07f, interval * 0.84f);
            }
            float phase = 0f;
            int from = (int)((length - 0.38f) * Rate);
            for (int i = from; i < s.Length; i++)
            {
                float k = (float)(i - from) / (s.Length - from);
                phase += 2f * Mathf.PI * Mathf.Lerp(1046f, 1568f, k) / Rate;
                float env = Mathf.Clamp01(k * 20f) * Mathf.Clamp01((1f - k) * 30f);
                s[i] += (Mathf.Sin(phase) + Mathf.Sin(phase * 3f) * 0.2f) * 0.45f * env;
            }
            return s;
        }

        static void AddBeep(float[] s, float start, float length, float freq, float amp)
        {
            int a = (int)(start * Rate), n = (int)(length * Rate);
            for (int i = 0; i < n && a + i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float env = Mathf.Clamp01(i / 200f) * Mathf.Clamp01((n - i) / 400f);
                s[a + i] += (Mathf.Sin(2f * Mathf.PI * freq * t) + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.2f) * amp * env;
            }
        }

        static float[] GuardianCharge()
        {
            var s = Buffer(1.25f);
            var rng = new System.Random(3);
            float phase = 0f, lp = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float k = t / 1.25f;
                float freq = 220f * Mathf.Pow(1200f / 220f, k) * (1f + 0.03f * Mathf.Sin(2f * Mathf.PI * 18f * t));
                phase += 2f * Mathf.PI * freq / Rate;
                lp = LowPass(lp, Noise(rng), Mathf.Lerp(300f, 4000f, k));
                float env = Mathf.SmoothStep(0f, 1f, k) * Mathf.Clamp01((1f - k) * 40f);
                s[i] = (Mathf.Sin(phase) * 0.5f + Mathf.Sin(phase * 2f) * 0.15f + lp * 0.8f) * env;
            }
            return s;
        }

        static float[] GuardianShot()
        {
            var s = Buffer(0.7f);
            var rng = new System.Random(4);
            float phase = 0f, prev = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                phase += 2f * Mathf.PI * (150f + 1650f * Mathf.Exp(-t * 9f)) / Rate;
                float zap = (Mathf.Sin(phase) + Mathf.Sign(Mathf.Sin(phase * 0.5f)) * 0.25f) * Mathf.Exp(-t / 0.18f);
                float noise = Noise(rng);
                float hiss = (noise - prev) * Mathf.Exp(-t / 0.08f); // paso alto casero
                prev = noise;
                s[i] = zap + hiss * 0.6f;
            }
            return s;
        }

        static float[] ArrowWhoosh()
        {
            var s = Buffer(0.35f);
            var rng = new System.Random(5);
            float lpA = 0f, lpB = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float k = t / 0.35f;
                float noise = Noise(rng);
                lpA = LowPass(lpA, noise, Mathf.Lerp(800f, 4000f, k));
                lpB = LowPass(lpB, noise, Mathf.Lerp(200f, 900f, k));
                float env = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Min(k * 1.1f, 1f)), 2f);
                s[i] = (lpA - lpB) * env * 2f;
            }
            return s;
        }

        // Zumbido grave + "succión" que sube y un "pop" al colapsar (a los 1,08 s).
        static float[] ArrowPortal()
        {
            var s = Buffer(1.5f);
            var rng = new System.Random(6);
            float lp = 0f, phase = 0f;
            const float pop = 1.08f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / Rate;
                float k = Mathf.Clamp01(t / pop);
                float drone = (Mathf.Sin(2f * Mathf.PI * 70f * t) + Mathf.Sin(2f * Mathf.PI * 105f * t) * 0.6f) * 0.35f;
                lp = LowPass(lp, Noise(rng), Mathf.Lerp(200f, 3000f, k * k));
                float suck = lp * 1.4f * k * k;
                float body = (drone + suck) * Mathf.Clamp01(t * 8f) * (t < pop ? 1f : Mathf.Exp(-(t - pop) / 0.03f));
                float popSound = 0f;
                if (t >= pop)
                {
                    float tp = t - pop;
                    phase += 2f * Mathf.PI * (120f + 780f * Mathf.Exp(-tp * 25f)) / Rate;
                    popSound = Mathf.Sin(phase) * Mathf.Exp(-tp / 0.12f) * 1.2f;
                }
                s[i] = body + popSound;
            }
            return s;
        }

        // ------------------------------------------------------------ utilidades

        static float[] Buffer(float seconds) => new float[(int)(seconds * Rate)];

        static float Noise(System.Random rng) => (float)rng.NextDouble() * 2f - 1f;

        static float LowPass(float state, float input, float cutoff)
        {
            float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / Rate);
            return state + a * (input - state);
        }

        static void Save(string name, float[] samples)
        {
            // Normaliza a -1,5 dB y añade un fundido mínimo para evitar clics.
            float peak = 1e-5f;
            foreach (var x in samples)
                peak = Mathf.Max(peak, Mathf.Abs(x));
            float gain = 0.84f / peak;
            int fade = Mathf.Min(256, samples.Length / 4);

            string path = $"{Folder}/{name}.wav";
            using (var stream = new FileStream(path, FileMode.Create))
            using (var w = new BinaryWriter(stream))
            {
                int dataBytes = samples.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + dataBytes);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);      // PCM
                w.Write((short)1);      // mono
                w.Write(Rate);
                w.Write(Rate * 2);
                w.Write((short)2);
                w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(dataBytes);
                for (int i = 0; i < samples.Length; i++)
                {
                    float env = Mathf.Min(1f, Mathf.Min(i, samples.Length - 1 - i) / (float)fade);
                    w.Write((short)Mathf.Clamp(samples[i] * gain * env * 32767f, -32768f, 32767f));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
