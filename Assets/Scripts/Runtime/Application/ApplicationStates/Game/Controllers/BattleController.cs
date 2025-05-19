using Core;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BattleController : BaseController<FoolGameRequest>
{
    private readonly BoardModel _boardModel;
    private readonly PlayerBattleModel _playerBattleOrderModel;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger _logger;

    public BattleController(
        BoardModel boardModel,
        PlayerBattleModel playerBattleOrderModel,
        ISettingProvider settingProvider,
        ILogger logger)
    {
        _boardModel = boardModel;
        _playerBattleOrderModel = playerBattleOrderModel;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    public override async UniTask Run(FoolGameRequest data, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        BasePlayer attacker = data.Players[_playerBattleOrderModel.AttackerID];
        BasePlayer defender = data.Players[_playerBattleOrderModel.DefenderID];

        _boardModel.StartRound(attacker, defender);

        while(!cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await attacker.ProcessAttack(_boardModel, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            bool result = await defender.ProcessDefense(_boardModel,cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            _playerBattleOrderModel.SetBattleResult(result);

            break;
        }

        await UniTask.WaitForSeconds(_settingProvider.Get<FoolGameConfig>().TurnDelay);
        cancellationToken.ThrowIfCancellationRequested();
    }
}