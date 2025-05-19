using System.Collections.Generic;
using UnityEngine;

namespace Core.Services
{
    [CreateAssetMenu(fileName = "ValidationConfig", menuName = "Config/ValidationConfig")]
    public sealed class UserValidationConfig : BaseSettings
    {
        [TextArea]
        public string UsernameRegex = @"^[a-zA-Z][a-zA-Z0-9_]{2,15}$";

        [Min(12)]
        public int MinAge = 12;
        public int MaxAge = 100;

        private void OnValidate()
        {
            if(MaxAge <= MinAge)
                MaxAge = MinAge + 1;
        }
    }
}