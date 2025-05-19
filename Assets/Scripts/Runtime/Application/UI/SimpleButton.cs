using Application.Services.Audio;
using Core.Services.Audio;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Application.UI
{
    public class SimpleButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Animation _pressAnimation;
        [SerializeField] private bool _playAnim = true;
        [SerializeField] private bool _playSound = true;

        private IAudioService _audioService;
        public Button Button => _button;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void PlayPressAnimation()
        {
            if(_playAnim)
                _pressAnimation.Play();

            if( _playSound )
                _audioService.PlaySound(ConstAudio.PressButtonSound);
        }
    }
}