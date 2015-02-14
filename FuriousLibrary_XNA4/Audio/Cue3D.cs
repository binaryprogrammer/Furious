using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace FuriousLibrary_XNA4.Audio
{
    public class Cue3D
    {
        /// <summary>
        /// the sound cue that was played
        /// </summary>
        public readonly Cue cue;

        /// <summary>
        /// the source of the sound
        /// </summary>
        public readonly AudioEmitter emitter;

        /// <summary>
        /// where the sound is being heard from
        /// </summary>
        public readonly AudioListener listener;

        internal Cue3D(Cue cue, AudioEmitter emitter, AudioListener listener)
        {
            this.cue = cue;
            this.emitter = emitter;
            this.listener = listener;
        }

        /// <summary>
        /// update only needs to happen in the listener or emitter changes position
        /// </summary>
        internal void Update()
        {
            cue.Apply3D(listener, emitter);
        }
    }
}
