using Core;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ChooseFirstPlayerController : BaseController<FoolGameRequest>
{
    private readonly BoardModel _boardModel;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger _logger;

    private PlayerBattleModel _playerBattleModel;

    public ChooseFirstPlayerController(
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
        cancellationToken.ThrowIfCancellationRequested();

        int index = FindPlayerIndexWithLowestTrumpCard(data.Players, _boardModel.TrumpCard);

        _playerBattleModel.SetFirstAttacker(index);

        await UniTask.WaitForSeconds(_settingProvider.Get<FoolGameConfig>().TurnDelay);
    }

    private int FindPlayerIndexWithLowestTrumpCard(BasePlayer[] players, Card trumpCard)
    {
        int playerIndexWithLowestTrump = -1;
        Card lowestTrumpCard = null;

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];

            foreach (var card in player.CardsAtHand)
            {
                if (card.CardSuit == trumpCard.CardSuit)
                {
                    if (lowestTrumpCard == null || card.CardValue < lowestTrumpCard.CardValue)
                    {
                        lowestTrumpCard = card;
                        playerIndexWithLowestTrump = i;
                    }
                }
            }
        }

        if (playerIndexWithLowestTrump == -1)
            return UnityEngine.Random.Range(0, players.Length);

        return playerIndexWithLowestTrump;
    }
}