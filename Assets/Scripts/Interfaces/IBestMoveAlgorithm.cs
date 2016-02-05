public interface IBestMoveAlgorithm
{
    void RunAlgorithm(GameBoard currentBoard);

    bool IsAlgorithmFinished();

    bool IsAlgorithmRunning();

    Move GetResult();
}
