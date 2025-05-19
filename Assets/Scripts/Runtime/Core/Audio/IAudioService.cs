using UnityEngine;

namespace Core.Services.Audio
{
    public interface IAudioService
    {
        void PlayMusic(string clipId);
        void PlayMusic(AudioClip clip);
        void PlaySound(string clipId);
        void StopMusic();
        void StopAll();
        void SetVolume(AudioType audioType, int volume);
    }
}