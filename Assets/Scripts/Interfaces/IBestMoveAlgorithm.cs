public interface IBestMoveAlgorithm
{
    void RunAlgorithm();

    bool IsRunning();

    bool IsFinished();

    bool InErrorState();

    object GetResult();
}
