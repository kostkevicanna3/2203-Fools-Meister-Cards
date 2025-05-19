using System.Collections.Generic;
using Core.UI;
using UnityEngine;

namespace Application.UI
{
    public sealed class UiServiceViewContainer : MonoBehaviour
    {
        [SerializeField] private ScreenFade _screenFade;
        [SerializeField] private List<UiScreen> _screensPrefab;
        [SerializeField] private List<BasePopup> _popupsPrefab;
        [SerializeField] private GameObject _avatarSelectionButtonPrefab;
        [SerializeField] private Transform _screenParent;

        public ScreenFade ScreenFade => _screenFade;
        public List<UiScreen> ScreensPrefab => _screensPrefab;
        public List<BasePopup> PopupsPrefab => _popupsPrefab;
        public GameObject AvatarSelectionButtonPrefab => _avatarSelectionButtonPrefab;
        public Transform ScreenParent => _screenParent;
    }
}