using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace FuriousLibrary_XNA4.Audio
{
    public class AudioSystem
    {
        AudioEngine _audioEngine;
        SoundBank _soundBank;
        WaveBank _waveBank;

        public List<AudioListener> audioListeners = new List<AudioListener>();

        List<Cue3D> _cue3Ds = new List<Cue3D>();

        public AudioSystem(string xgsPath, string xwbPath, string xsbPath)
        {
            _audioEngine = new AudioEngine(xgsPath);
            _soundBank = new SoundBank(_audioEngine, xsbPath);
            _waveBank = new WaveBank(_audioEngine, xwbPath);
        }

        public void PlayCue(string cueName)
        {
            _soundBank.PlayCue(cueName);
        }

        public void PlayCue3D(string cueName, AudioEmitter audioEmitter)
        {
            for (int i = 0; i < audioListeners.Count; ++i)
            {
                Cue3D cue3D = new Cue3D(_soundBank.GetCue(cueName), audioEmitter, audioListeners[i]);
                cue3D.Update();
                cue3D.cue.Play();
                _cue3Ds.Add(cue3D);
            }
        }

        public void Update()
        {
            for (int i = _cue3Ds.Count - 1; i >= 0; --i)
            {
                if (_cue3Ds[i].cue.IsPlaying)
                {
                    _cue3Ds[i].Update();
                }
                else
                {
                    _cue3Ds.RemoveAt(i);
                }
            }
        }
    }
}
