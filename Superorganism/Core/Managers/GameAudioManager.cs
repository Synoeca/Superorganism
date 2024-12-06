using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Superorganism.Core.Managers
{
    public class GameAudioManager
    {
        private readonly SoundEffect _cropPickup;
        private readonly SoundEffect _fliesDestroy;
        private readonly Song _backgroundMusic;

        public GameAudioManager(ContentManager content)
        {
            _cropPickup = content.Load<SoundEffect>("Pickup_Coin4");
            _fliesDestroy = content.Load<SoundEffect>("damaged");
            _backgroundMusic = content.Load<Song>("MaxBrhon_Cyberpunk");
        }

        public void Initialize(float soundEffectVolume, float musicVolume)
        {
            SoundEffect.MasterVolume = soundEffectVolume;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = musicVolume;
            MediaPlayer.Play(_backgroundMusic);
        }

        public void PlayCropPickup() => _cropPickup.Play();
        public void PlayFliesDestroy() => _fliesDestroy.Play();

        // Optional: Add methods to control background music
        public void PauseMusic() => MediaPlayer.Pause();
        public void ResumeMusic() => MediaPlayer.Resume();
        public void StopMusic() => MediaPlayer.Stop();
    }
}