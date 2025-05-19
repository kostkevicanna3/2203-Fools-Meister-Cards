using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Application.UI
{
    public class SplashScreen : UiScreen
    {
        [SerializeField]  private GameObject[] _loadingProgressObjects;
        [SerializeField, Min(0.3f)] private float _loadingTime = 1.5f;

        public override async UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            UniTask task =  PlayLoadingAnimation(cancellationToken);
            await WaitSplashScreenAnimationFinish(cancellationToken);
            await task;
            await base.HideAsync(cancellationToken);
        }

        private async UniTask PlayLoadingAnimation(CancellationToken token)
        {
            int objectCount = _loadingProgressObjects.Length;
            List<float> randomIntervals = CalculateRandomIntervals(_loadingTime, objectCount);

            for (int i = 0; i < objectCount; i++)
            {
                await UniTask.Delay((int)(randomIntervals[i] * 1000), cancellationToken: token);
                _loadingProgressObjects[i].SetActive(true);
            }
        }

        private List<float> CalculateRandomIntervals(float totalTime, int count)
        {
            List<float> intervals = new List<float>();
            float sum = 0;

            for (int i = 0; i < count; i++)
            {
                float randomValue = Random.Range(0.1f, 1.0f); // Adjust range for variety in intervals
                intervals.Add(randomValue);
                sum += randomValue;
            }

            for (int i = 0; i < count; i++)
            {
                intervals[i] = (intervals[i] / sum) * totalTime;
            }

            return intervals;
        }

        private async UniTask WaitSplashScreenAnimationFinish(CancellationToken cancellationToken)
        {
            await UniTask.Delay(2000, cancellationToken: cancellationToken);
        }
    }
}