using System.Collections;
using UnityEngine;

namespace Orientation
{
    public class Orientation : MonoBehaviour
    {
        public enum OrientationMode
        {
            Portrait,
            Landscape,
            Both
        }

        [SerializeField]
        private OrientationMode orientationMode = OrientationMode.Both;

        protected void Awake()
        {
            ForcePortraitThenAutorotate();
        }

        private void ForcePortraitThenAutorotate()
        {
            Screen.orientation = orientationMode == OrientationMode.Portrait? ScreenOrientation.Portrait : ScreenOrientation.LandscapeLeft;

            StartCoroutine(EnableAutoRotationNextFrame());
        }

        private IEnumerator EnableAutoRotationNextFrame()
        {
            yield return null; 
            
            SetAutoRotate();
        }
        
        private void SetAutoRotate()
        {
            switch (orientationMode)
            {
                case OrientationMode.Portrait:
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                    Screen.autorotateToLandscapeLeft = false;
                    Screen.autorotateToLandscapeRight = false;
                    break;

                case OrientationMode.Landscape:
                    Screen.autorotateToPortrait = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    break;

                case OrientationMode.Both:
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    break;
            }

            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}