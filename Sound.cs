using NAudio.Wave;
using Rhino;
using System;
using System.IO;

namespace RhinoArkanoid
{
    static class Sound
    {
        private static WaveOutEvent _bwMusicOut;
        private static bool _playingBg;
        public static void PlayBgMusic(byte[] bytes)
        {
            if (bytes == null) return;
            _playingBg = false;
            _bwMusicOut?.Stop();
            _playingBg = true;

            try
            {
                var wav = new Mp3FileReader(new MemoryStream(bytes));
                _bwMusicOut = new WaveOutEvent();
                _bwMusicOut.PlaybackStopped += (o, e) =>
                {
                    _bwMusicOut.Dispose();
                    wav.Dispose();

                    if (_playingBg) PlayBgMusic(bytes);
                };
                _bwMusicOut.Init(wav);
                _bwMusicOut.Play();
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Sound Error: {ex.Message}");
            }
        }

        public static void StopBwMusic()
        {
            _playingBg = false;
            _bwMusicOut?.Stop();

        }
        //public static void ResetBwMusic()
        //{
        //    _bwMusicOut?.Stop();

        //}

        public static void Play(byte[] bytes)
        {
            if (bytes == null) return;
            try
            {
                var wav = new Mp3FileReader(new MemoryStream(bytes));
                var output = new WaveOutEvent();
                output.PlaybackStopped += (o, e) =>
                {
                    output.Dispose();
                    wav.Dispose();
                };
                output.Init(wav);
                output.Play();
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Sound Error: {ex.Message}");
            }
        }
    }
}
