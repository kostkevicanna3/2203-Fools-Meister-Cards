public class FoolGameRequest
{
    private BasePlayer[] _players;
    private bool _gameStarted;

    public BasePlayer[] Players => _players;
    public bool GameStarted => _gameStarted;

    public FoolGameRequest(BasePlayer[] players, bool gameStarted)
    {
        _players = players;
        _gameStarted = gameStarted;
    }

    public void StartGame() => _gameStarted = true;
}