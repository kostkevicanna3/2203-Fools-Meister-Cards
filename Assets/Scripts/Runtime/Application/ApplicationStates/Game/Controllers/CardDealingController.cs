using Core;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CardDealingController : BaseController<FoolGameRequest>
{
    private readonly BoardModel _boardModel;
    private readonly PlayerBattleModel _playerBattleModel;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger _logger;

    private CancellationToken _token;

    public CardDealingController(
        BoardModel boardModel,
        PlayerBattleModel playerBattleModel,
        ISettingProvider settingProvider,
        ILogger logger)
    {
        _boardModel = boardModel;
        _playerBattleModel = playerBattleModel;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    public override async UniTask Run(FoolGameRequest data, CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _token.ThrowIfCancellationRequested();

        if(!data.GameStarted)
        {
            FoolGameConfig gameConfig = _settingProvider.Get<FoolGameConfig>();
            bool freeCards = gameConfig.InitialCardsAtHand * data.Players.Length < gameConfig.MaxCardsInDeck;

            if (!freeCards)
                _boardModel.SelectTrumpCard();

            await GiveInitialCards(data);

            if (freeCards)
                _boardModel.SelectTrumpCard();
        }
        else
        {
            if(_playerBattleModel.LastRoundDefenderWon)
            {
                _boardModel.ClearBoard();
                await GiveMissingCardsTo(data.Players[_playerBattleModel.AttackerID]);
                await GiveMissingCardsTo(data.Players[_playerBattleModel.DefenderID]);
            }
            else
            {
                await GiveAllBoardCardsToDefender(data.Players[_playerBattleModel.DefenderID]);
                await GiveMissingCardsTo(data.Players[_playerBattleModel.AttackerID]);
            }
        }

        await UniTask.WaitForSeconds(_settingProvider.Get<FoolGameConfig>().TurnDelay);
    }

    private async UniTask GiveInitialCards(FoolGameRequest data)
    {
        int initialCardsAtHand = _settingProvider.Get<FoolGameConfig>().InitialCardsAtHand;
        foreach (BasePlayer player in data.Players)
        {
            for (int i = 0; i < initialCardsAtHand; i++)
            {
                _token.ThrowIfCancellationRequested();
                await _boardModel.GiveCardToPlayer(player);
                _token.ThrowIfCancellationRequested();
            }
        }
    }

    private async UniTask GiveAllBoardCardsToDefender(BasePlayer player)
    {
        _token.ThrowIfCancellationRequested();
        await _boardModel.GiveCardFromBoardToPlayer(player);
        _token.ThrowIfCancellationRequested();
    }

    private async UniTask GiveMissingCardsTo(BasePlayer player)
    {
        if (!player.InGame)
            return;

        int initialCardsAtHand = _settingProvider.Get<FoolGameConfig>().InitialCardsAtHand;

        while (player.CardsAtHand.Count < initialCardsAtHand)
        {
            if (!_boardModel.AnyCardsLeft)
                break;

            _token.ThrowIfCancellationRequested();
            await _boardModel.GiveCardToPlayer(player);
            _token.ThrowIfCancellationRequested();
        }
    }
}