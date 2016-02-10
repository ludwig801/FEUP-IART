public interface IBestMoveAlgorithm
{
    void RunAlgorithm(GameBoard board);

    bool IsRunning();

    bool IsFinished();

    Move GetResult();
}
