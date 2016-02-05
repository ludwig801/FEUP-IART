public interface IAlgorithm
{
    void RunAlgorithm();

    bool IsRunning();

    bool IsFinished();

    bool InErrorState();

    object GetResult();
}
