using System;
using System.IO;

namespace CyberSecurityChatbot
{
    public static class WavGenerator
    {
        /// <summary>
        /// Generates a valid welcome.wav audio file (retro digital chime) if it doesn't already exist.
        /// This ensures the application is completely self-contained and fulfills Task 1 audio requirements.
        /// </summary>
        public static void GenerateWelcomeWav(string filePath)
        {
            try
            {
                // Check if directory exists, create if not
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // If file already exists, don't overwrite it
                if (File.Exists(filePath))
                {
                    return;
                }

                // Audio configuration
                ushort numChannels = 1;       // Mono
                uint sampleRate = 11025;      // 11.025 kHz sample rate
                ushort bitsPerSample = 16;    // 16-bit audio
                double durationSeconds = 1.2; // 1.2 second chime

                int numSamples = (int)(sampleRate * durationSeconds);
                int dataSize = numSamples * numChannels * (bitsPerSample / 8);
                int fileSize = 36 + dataSize;

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    // 1. RIFF Header
                    bw.Write(new char[] { 'R', 'I', 'F', 'F' });
                    bw.Write(fileSize);
                    bw.Write(new char[] { 'W', 'A', 'V', 'E' });

                    // 2. Format Chunk
                    bw.Write(new char[] { 'f', 'm', 't', ' ' });
                    bw.Write(16); // Subchunk size (16 for PCM)
                    bw.Write((ushort)1); // Audio format (1 for PCM)
                    bw.Write(numChannels);
                    bw.Write(sampleRate);
                    // ByteRate = SampleRate * NumChannels * BitsPerSample/8
                    bw.Write(sampleRate * numChannels * (bitsPerSample / 8));
                    // BlockAlign = NumChannels * BitsPerSample/8
                    bw.Write((ushort)(numChannels * (bitsPerSample / 8)));
                    bw.Write(bitsPerSample);

                    // 3. Data Chunk
                    bw.Write(new char[] { 'd', 'a', 't', 'a' });
                    bw.Write(dataSize);

                    // Generate a nice dual-tone futuristic cyber chime (synthesizing sine waves)
                    for (int i = 0; i < numSamples; i++)
                    {
                        double t = (double)i / sampleRate;
                        
                        // Volume envelope (fades out smoothly)
                        double envelope = Math.Exp(-3.0 * t); // decay

                        // Tone frequencies: C5 (523.25 Hz) and G5 (783.99 Hz)
                        double freq1 = 523.25;
                        double freq2 = 783.99;

                        // Slide effect (pitch going up slightly to sound "optimistic")
                        double slide = 1.0 + (0.05 * t);
                        
                        double val1 = Math.Sin(2 * Math.PI * freq1 * slide * t);
                        double val2 = Math.Sin(2 * Math.PI * freq2 * slide * t);

                        // Mix and scale to 16-bit PCM range (-32768 to 32767)
                        double mixed = (val1 + val2) * 0.5 * envelope;
                        short sampleValue = (short)(mixed * 32000);

                        bw.Write(sampleValue);
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently log or handle audio generation failures so it never crashes the application
                Console.WriteLine($"WAV Generation Warning: {ex.Message}");
            }
        }
    }
}
