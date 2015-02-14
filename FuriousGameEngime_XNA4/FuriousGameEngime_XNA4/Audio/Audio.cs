//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using FuriousLibrary_XNA4;
//using System.IO;

//namespace FuriousGameEngime_XNA4.Audio
//{
//    class CAudio
//    {
//        //protected CGame _game;

//        //internal AudioEmitter audioEmitter;

//        internal SoundEffect[] sounds;

//        internal string[] names;

//        internal CAudio(CGame game)
//        {
//            _game = game;
            
//            //audioEmitter = new AudioEmitter();
//            //audioEmitter.Position = Vector3.Zero;
            
//            //Don't need to load audio in this fashion
//            //LoadAudio();
//        }

//        internal void LoadAudio()
//        {
//            string[] names = _game.GetAllFileNames("Content//Sound//SteroSounds", "*.xnb");
//            List<SoundEffect> soundEffects = new List<SoundEffect>();

//            for (int i = 0; i < names.Length; ++i)
//            {
//                soundEffects.Add(_game.Content.Load<SoundEffect>(Path.GetDirectoryName(names[i]) + "\\" + Path.GetFileNameWithoutExtension(names[i])));
//            }
//            sounds = soundEffects.ToArray();
//        }

//        #region Play Functions
//        /// <summary>
//        /// picks a random sound and plays it. loop = false.
//        /// </summary>
//        /// <param name="soundEffect"> an array of sounds to pick from</param>
//        /// <param name="volume"> the volume the sound is played at</param>
//        internal void PlayRandom(SoundEffect[] soundEffect, float volume)
//        {
//            int index = CRandom.RandomIntBetween(0, soundEffect.Length - 1);
//            Play(soundEffect[index], volume, 0, 0, false);
//        }

//        /// <summary>
//        /// picks a random sound and plays it. loop = false.
//        /// </summary>
//        /// <param name="soundEffect"> an array of sounds to pick from</param>
//        /// <param name="volume"> the volume the sound is played at</param>
//        /// <param name="pan"> the pan the sound is played at</param>
//        internal void PlayRandom(SoundEffect[] soundEffect, float volume, float pan)
//        {
//            int index = CRandom.RandomIntBetween(0, soundEffect.Length - 1);
//            Play(soundEffect[index], volume, pan, 0, false);
//        }

//        /// <summary>
//        /// picks a random sound and plays it. loop = false.
//        /// </summary>
//        /// <param name="soundEffect"> an array of sounds to pick from</param>
//        /// <param name="volume"> the volume the sound is played at</param>
//        /// <param name="pan"> the pan the sound is played at</param>
//        /// <param name="pitch"> the pitch the sound is played at</param>
//        internal void PlayRandom(SoundEffect[] soundEffect, float volume, float pan, float pitch)
//        {
//            int index = CRandom.RandomIntBetween(0, soundEffect.Length - 1);
//            Play(soundEffect[index], volume, pan, pitch, false);
//        }

//        /// <summary>
//        /// plays the specified sound
//        /// </summary>
//        /// <param name="sound"> the sound to be played</param>
//        /// <param name="volume"> the volume to be played at</param>
//        internal void Play(SoundEffect sound, float volume)
//        {
//            Play(sound, volume, 0, 0, true);
//        }

//        /// <summary>
//        /// plays the specified sound
//        /// </summary>
//        /// <param name="sound"> the sound to be played</param>
//        /// <param name="volume"> the volume to be played at</param>
//        /// <param name="pan"> the pan to be played at</param>
//        internal void Play(SoundEffect sound, float volume, float pan)
//        {
//            Play(sound, volume, pan, 0, true);
//        }

//        /// <summary>
//        /// plays the specified sound
//        /// </summary>
//        /// <param name="sound"> the sound to be played</param>
//        /// <param name="volume"> the volume to be played at</param>
//        /// <param name="pan"> the pan to be played at</param>
//        /// <param name="pitch"> the pitch to be played at</param>
//        internal void Play(SoundEffect sound, float volume, float pan, float pitch)
//        {
//            Play(sound, volume, pan, pitch, true);
//        }

//        /// <summary>
//        /// plays the specified sound
//        /// </summary>
//        /// <param name="sound"> the sound to be played</param>
//        /// <param name="volume"> the volume to be played at</param>
//        /// <param name="pan"> the pan to be played at</param>
//        /// <param name="pitch"> the pitch to be played at</param>
//        /// <param name="looped"> loop the sound?</param>
//        internal void Play(SoundEffect sound, float volume, float pan, float pitch, bool looped)
//        {
//            SoundEffectInstance instance = sound.CreateInstance();
//            instance.Volume = volume;
//            instance.Pitch = pitch;
//            instance.Pan = pan;
//            instance.IsLooped = looped;
//            instance.Play();
//        }
//        #endregion

//        //Checks if condition to play sound has been met
//        internal void Tick()
//        {
            
//        }

//        internal SoundEffect GetSoundFromName(string name)
//        {
//            for (int i = 0; i < names.Length; ++i)
//            {
//                if (names[i] == name)
//                {
//                    return sounds[i];
//                }
//            }
//            throw new IndexOutOfRangeException("Could not find " + name + ".");
//        }

//        internal int GetIndexFromName(string name)
//        {
//            for (int i = 0; i < name.Length; ++i)
//            {
//                if (names[i] == name)
//                {
//                    return i;
//                }
//            }
//            throw new IndexOutOfRangeException("Could not find " + name + ".");
//        }
//    }
//}