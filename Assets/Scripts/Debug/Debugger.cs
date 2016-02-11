using UnityEngine;
using System.Collections;
using System.IO;

public class Debugger : MonoBehaviour
{
    [Range(1, 3)]
    public int MinimaxDepth;
    [Range(0, 0)]
    public float MinWeight;
    [Range(1, 5)]
    public float MaxWeight;
    [Range(0, 1)]
    public float WeightIncrement;
    [Range(2, 50)]
    public int NumGamesPerTest;

    public float[] HeuristicWeightsPlayer, HeuristicWeightsOpponent;
    [SerializeField]
    bool _running, _noMoreWeightsToEvaluate;
    [SerializeField]
    GameBoard _board;
    [SerializeField]
    int _gameCount, _localVictoriesStarter, _localVictoriesNonStarter, _localLossesStarter, _localLossesNonStarter, _localTies, _startingPlayer;
    float _counter;
    string _savePath;

    void Update()
    {
        if (_running)
        {
            _counter += Time.deltaTime;

            if (_board.Ongoing)
            {
                if (_board.IsGameOver)
                {
                    OnGameOver();
                    _board.Ongoing = false;
                    _counter = 0;
                }
            }
            else if(_counter > 1f)
            {
                _board.Minimax.Depth = MinimaxDepth;
                _startingPlayer = _gameCount < NumGamesPerTest / 2 ? 0 : 1;
                _board.StartingPlayer = _startingPlayer;
                _board.GetPlayer(0).IsCpu = true;
                _board.GetPlayer(1).IsCpu = true;
                _board.NewGame();
            }
        }
    }

    public void RunTests()
    {
        var found = false;
        for (var i = 0; i < 10000; i++)
        {
            var path = Names.WeightsDebugPath_ + i + Names.SaveDebugExt;
            if (!File.Exists(path))
            {
                _savePath = path;
                found = true;
                break;
            }
        }
        if (!found)
        {
            for (var i = 0; i < 10000; i++)
            {
                var path = Names.WeightsDebugPath_ + i + Names.SaveDebugExt;
                File.Delete(path);
            }
            _savePath = Names.WeightsDebugPath_ + 0 + Names.SaveDebugExt;
        }

        StreamWriter stream = new StreamWriter(_savePath, true);
        stream.WriteLine("\"Game\",\"Favourable\",\"Player Weights\",\"Opponent Weights\"," +
            "\"Moves\",\"Games\",\"Victories (Starting Player)\",\"Victories (Nont-starting Player)\"," +
            "\"Losses (Starting Player)\",\"Losses (Nont-starting Player)\",\"Ties\"");
        stream.Close();

        _counter = 0;

        ResetWeightsFor(0);
        ResetWeightsFor(1);

        _running = true;
        _noMoreWeightsToEvaluate = false;
        _gameCount = 0;
        _localVictoriesStarter = 0;
        _localLossesStarter = 0;
        _localTies = 0;
    }

    void ResetWeightsFor(int player)
    {
        if (player == 0)
        {
            HeuristicWeightsPlayer[0] = WeightIncrement;
            HeuristicWeightsPlayer[1] = 0;
            HeuristicWeightsPlayer[2] = 0;
            HeuristicWeightsPlayer[3] = 0;
        }
        else
        {
            HeuristicWeightsOpponent[0] = WeightIncrement;
            HeuristicWeightsOpponent[1] = 0;
            HeuristicWeightsOpponent[2] = 0;
            HeuristicWeightsOpponent[3] = 0; 
        }
    }

    void OnGameOver()
    {
        if (_noMoreWeightsToEvaluate)
            _running = false;

        _gameCount++;

        CollectLocalResults();

        if (_gameCount >= NumGamesPerTest)
        {
            _gameCount = 0;
            CollectGlobalResults();
            CalculateNextWeights();
        }
    }

    void CalculateNextWeights()
    {
        var increaseForPlayer = false;

        for (var i = 0; i < 4; i++)
        {
            if (HeuristicWeightsOpponent[i] < MaxWeight)
            {
                HeuristicWeightsOpponent[i] = Mathf.Min(MaxWeight, HeuristicWeightsOpponent[i] + WeightIncrement);
                break;
            }
            else if (i < 3)
            {
                for (var j = 0; j < 3; j++)
                {
                    HeuristicWeightsOpponent[i] = MinWeight;
                }
            }
            else
            {
                ResetWeightsFor(1);
                increaseForPlayer = true;
            }
        }

        if (increaseForPlayer)
        {
            for (var i = 0; i < 4; i++)
            {
                if (HeuristicWeightsPlayer[i] < MaxWeight)
                {
                    HeuristicWeightsPlayer[i] = Mathf.Min(MaxWeight, HeuristicWeightsPlayer[i] + WeightIncrement);
                    break;
                }
                else if (i < 3)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        HeuristicWeightsPlayer[i] = MinWeight;
                    }
                }
                else
                {
                    _noMoreWeightsToEvaluate = true;
                }
            }
        }
    }

    void CollectLocalResults()
    {
        if (_board.Winner < 0)
            _localTies++;
        else if (_board.Winner == 0)
        {
            if (_startingPlayer == 0)
                _localVictoriesStarter++;
            else
                _localVictoriesNonStarter++;
        }
        else
        {
            if(_startingPlayer == 0)
                _localLossesStarter++;
            else
                _localLossesNonStarter++;
        }
    }

    void CollectGlobalResults()
    {
        StreamWriter stream = new StreamWriter(_savePath, true);

        var favourable = (_localVictoriesStarter + _localVictoriesNonStarter) - (_localLossesStarter + _localLossesNonStarter);
        stream.WriteLine("Game," + (favourable > 0 ? "true" : favourable < 0 ? "false" : "-") +
            "," + HeuristicWeightsPlayer[0] + "," + HeuristicWeightsPlayer[1] + "," + HeuristicWeightsPlayer[2] + "," + HeuristicWeightsPlayer[3] + 
            "," + HeuristicWeightsOpponent[0] + "," + HeuristicWeightsOpponent[1] + "," + HeuristicWeightsOpponent[2] + "," + HeuristicWeightsOpponent[3] +
            "," + _board.MoveCount +
            "," + NumGamesPerTest +
            "," + _localVictoriesStarter + "," + _localVictoriesNonStarter +
            "," + _localLossesStarter + "," + _localLossesNonStarter + 
            "," + _localTies);
        stream.Close();
    }
}
