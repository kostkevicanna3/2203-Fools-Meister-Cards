using System;
using System.Collections.Generic;
using Core;
using Core.Services;
using Core.Services.Audio;
using Core.Services.ScreenOrientation;
using Cysharp.Threading.Tasks;

namespace Application.Services
{
    public class SettingsProvider : ISettingProvider
    {
        private readonly IAssetProvider _assetProvider;

        private Dictionary<Type, BaseSettings> _settings = new Dictionary<Type, BaseSettings>();

        public SettingsProvider(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        public async UniTask Initialize()
        {
            var audioConfig = await _assetProvider.Load<AudioConfig>(ConstConfigs.AudioConfig);
            Set(audioConfig);

            var validationConfig = await _assetProvider.Load<UserValidationConfig>(ConstConfigs.ValidationConfig);
            Set(validationConfig);

            var avatarSelectionConfig = await _assetProvider.Load<AvatarSelectionConfig>(ConstConfigs.AvatarSelectionConfig);
            Set(avatarSelectionConfig);

            var foolGameConfig = await _assetProvider.Load<FoolGameConfig>(ConstConfigs.FoolGameConfig);
            Set(foolGameConfig);

            var botNamesConfig = await _assetProvider.Load<BotNameConfig>(ConstConfigs.BotNameConfig);
            Set(botNamesConfig);

            var botThoughts = await _assetProvider.Load<BotRoundThoughtsConfig>(ConstConfigs.BotThoughts);
            Set(botThoughts);

            var orientationConfig = await _assetProvider.Load<ScreenOrientationConfig>(ConstConfigs.ScreenOrientationConfig);
            Set(orientationConfig);
        }

        public T Get<T>() where T : BaseSettings
        {
            if (_settings.ContainsKey(typeof(T)))
            {
                var setting = _settings[typeof(T)];
                return setting as T;
            }

            throw new Exception("No setting found");
        }

        public void Set(BaseSettings config)
        {
            if (_settings.ContainsKey(config.GetType()))
                return;

            _settings.Add(config.GetType(), config);
        }
    }
}