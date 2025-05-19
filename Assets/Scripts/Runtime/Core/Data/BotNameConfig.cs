using System.Collections.Generic;
using UnityEngine;

namespace Core.Services
{
    [CreateAssetMenu(fileName = "BotNameConfig", menuName = "Config/BotNameConfig")]
    public sealed class BotNameConfig : BaseSettings
    {
        [SerializeField]
        private List<string> _botNames;

        public List<string> BotNames => _botNames;
    }
}