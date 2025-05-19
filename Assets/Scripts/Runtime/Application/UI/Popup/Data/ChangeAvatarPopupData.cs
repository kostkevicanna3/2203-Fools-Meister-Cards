using Core.UI;
using System.Collections.Generic;

namespace Application.UI
{
    public class ChangeAvatarPopupData : BasePopupData
    {
        public List<AvatarData> AvatarsData;
        public string SelectedAvatarAssetName;
    }
}