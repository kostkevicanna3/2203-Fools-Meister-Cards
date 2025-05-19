using Core.Services.ScreenOrientation;
using UnityEngine;
using Zenject;

namespace Application.Game
{
    [CreateAssetMenu(fileName = "GameInstaller", menuName = "Installers/GameInstaller")]
    public class GameInstaller : ScriptableObjectInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ScreenOrientationAlertController>().AsSingle().NonLazy();

            Container.Bind<MenuStateController>().AsSingle();
            Container.Bind<ProfileMenuStateController>().AsSingle();
            Container.Bind<FoolGameController>().AsSingle();
            Container.Bind<BoardModel>().AsSingle();
            Container.Bind<CardDeck>().AsSingle();

            Container.Bind<CardDealingController>().AsSingle();
            Container.Bind<ChooseFirstPlayerController>().AsSingle();
            Container.Bind<BattleController>().AsSingle();
            Container.Bind<PlayerBattleModel>().AsSingle();
        }
    }
}