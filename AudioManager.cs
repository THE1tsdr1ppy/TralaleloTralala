using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace TralaleroTralala
{
    internal class AudioManager
    {
        private readonly Dictionary<string, string> _audioFiles = new Dictionary<string, string>
        {
            { "click", "click.wav" },
            { "error", "error.wav" },
            { "notification", "notification.wav" }
        };
        public Form1 form1;
        private float _volume = 1.0f;
        private bool _isMuted = false;
        private Dictionary<string, SoundPlayer> _soundEffects;

        // Volume property with clamping
        public float Volume
        {
            get => _volume;
            set => _volume = Clamp(value, 0.0f, 1.0f);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => _isMuted = value;
        }

        public AudioManager(Form1 form1)
        {
            this.form1 = form1;
            _soundEffects = new Dictionary<string, SoundPlayer>();
            InitializeSounds();
        }

        private void InitializeSounds()
        {
            try
            {
                // Define paths to sound files
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string soundsDir = Path.Combine(baseDir, "Sounds");

                // Create directory if it doesn't exist
                if (!Directory.Exists(soundsDir))
                {
                    Directory.CreateDirectory(soundsDir);
                }

                // Define sound file paths
                Dictionary<string, string> soundPaths = new Dictionary<string, string>
                {
                    { "TabOpen", Path.Combine(soundsDir, "tab_open.wav") },
                    { "TabClose", Path.Combine(soundsDir, "tab_close.wav") },
                    { "Navigation", Path.Combine(soundsDir, "navigation.wav") }
                };

                // Load sounds if they exist
                foreach (var sound in soundPaths)
                {
                    if (File.Exists(sound.Value))
                    {
                        _soundEffects[sound.Key] = new SoundPlayer(sound.Value);
                        _soundEffects[sound.Key].Load();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sounds: {ex.Message}");
            }
        }

        public void PlaySound(string soundName)
        {
            if (_isMuted || !_soundEffects.ContainsKey(soundName)) return;

            try
            {
                _soundEffects[soundName].Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound {soundName}: {ex.Message}");
            }
        }

        public void StopAllSounds()
        {
            foreach (var sound in _soundEffects.Values)
            {
                try
                {
                    sound.Stop();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping sound: {ex.Message}");
                }
            }
        }

        // Implementation of Math.Clamp for .NET Framework compatibility
        private T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }
    }
}
