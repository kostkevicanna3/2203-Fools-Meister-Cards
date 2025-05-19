using System.Collections.Generic;

namespace Application.Tools
{
    public class Tools
    {
        public static void Shuffle<T>(List<T> list) where T : class
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var audioClip = list[i];
                var randomClipIndex = UnityEngine.Random.Range(0, count);
                list[i] = list[randomClipIndex];
                list[randomClipIndex] = audioClip;
            }
        }
    }
}