using UnityEngine;
using System.Collections;
using System.IO;

[RequireComponent(typeof(GameManager))]
public class Tester : MonoBehaviour
{
    public int numGamesPerPlayer;
    public int maxRounds;
    public bool overrideMaxRounds;

    public bool ongoingRounds;
    bool stopProgram;
    GameManager _gameManager;

    int fileIndex;

    // Statistical variables
    int player;
    float[,] weight;
    float[] weightInit;
    float[] weightInc;
    float[] weightMax;
    Counter[] numGames;
    Counter numRounds;
    Counter wins;
    Counter winsTotal;
    Counter losses;
    Counter draws;

    // Use this for initialization
    void Start()
    {
        _gameManager = GameManager.Instance;
        InitializeVars();
    }

    void InitializeVars()
    {
        ongoingRounds = false;
        numGames = new Counter[_gameManager.NumPlayers];
        for (int i = 0; i < numGames.Length; i++) { numGames[i] = new Counter(); }
        numRounds = new Counter();
        numRounds.Inc();
        wins = new Counter();
        winsTotal = new Counter();
        losses = new Counter();
        draws = new Counter();

        weight = new float[_gameManager.Minimax.NumEvalFeatures, _gameManager.NumPlayers];
        weightInit = new float[_gameManager.Minimax.NumEvalFeatures];
        weightInc = new float[_gameManager.Minimax.NumEvalFeatures];
        weightMax = new float[_gameManager.Minimax.NumEvalFeatures];

        weightInc[0] = 0.2f;
        weightInc[1] = 0.2f;
        weightInc[2] = 0.1f;
        weightInc[3] = 0.1f;

        weightMax[0] = 3f;
        weightMax[1] = 3f;
        weightMax[2] = 1f;
        weightMax[3] = 1f;

        weight[0, 0] = 1f;
        weight[1, 0] = 0f;
        weight[2, 0] = 0f;
        weight[3, 0] = 0f;
        weightInit[0] = weight[0, 0];
        weightInit[1] = weight[1, 0];
        weightInit[2] = weight[2, 0];
        weightInit[3] = weight[3, 0];

        weight[0, 1] = 1f;
        weight[1, 1] = 1f;
        weight[2, 1] = 1f;
        weight[3, 1] = 1f;

        _gameManager.Minimax.SetWeights(weight);

        stopProgram = false;
        player = 0;

        string path;
        fileIndex = -1;

        do
        {
            fileIndex++;
            path = Names.SavePath_ + fileIndex + Names.SaveExt;
        } while (File.Exists(path));
    }

    // Update is called once per frame
    void Update()
    {
        if (ongoingRounds)
        {
            if (!stopProgram)
            {
                //Debug.Log("Game State: " + manager.gameState);
                switch (_gameManager.CurrentGameState)
                {
                    case GameManager.GameState.Ongoing:
                        //Debug.Log("Game " + player + " Over!");
                        break;
                    case GameManager.GameState.Stopped:
                        _gameManager.NewGame(player);
                        numGames[player].Inc();
                        break;
                    case GameManager.GameState.Over:
                        UpdateInfo();
                        //manager.NewGame(player);
                        //ongoingRounds = false;
                        break;
                    case GameManager.GameState.Error:
                        UpdateInfo();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }
    }

    void SaveHeaderToFile(bool full = true)
    {
        StreamWriter stream = new StreamWriter(Names.SavePath_ + fileIndex + Names.SaveExt, true);
        if (full) stream.WriteLine("==============================================");
        if (full) stream.WriteLine("Round : " + numRounds.Value);
        if (full) stream.WriteLine("Games : " + numGamesPerPlayer);
        if (full) stream.WriteLine("Feats : [ " + weight[0, 0] + " | " + weight[1, 0] + " | " + weight[2, 0] + " | " + weight[3, 0] + " ]");
        stream.WriteLine("---------------");
        stream.WriteLine("Starting : " + player);
        stream.WriteLine(">");
        stream.Close();
    }

    void SaveStatisticsToFile()
    {
        StreamWriter stream = new StreamWriter(Names.SavePath_ + fileIndex + Names.SaveExt, true);
        stream.WriteLine(">");
        stream.WriteLine("Wins   : " + wins.Value);
        stream.WriteLine("Losses : " + losses.Value);
        stream.WriteLine("Draws  : " + draws.Value);
        stream.Close();
    }

    void SaveGameToFile()
    {
        StreamWriter stream = new StreamWriter(Names.SavePath_ + fileIndex + Names.SaveExt, true);
        if (_gameManager.CurrentGameState == GameManager.GameState.Draw)
        {
            stream.WriteLine("-");
            draws.Inc();
        }
        else if (_gameManager.Winner == 0)
        {
            wins.Inc();
            winsTotal.Inc();
        }
        else
        {
            losses.Inc();
        }
        stream.Close();
    }

    void ResetCounters()
    {
        wins.Reset();
        losses.Reset();
        draws.Reset();
    }

    void NextPlayer()
    {
        player++;
        //player %= GameManager.NumPlayers;
    }

    void NextRound()
    {
        if (overrideMaxRounds && (numRounds.Value >= maxRounds))
        {
            stopProgram = true;
        }
        else
        {
            numRounds.Inc();
            player = 0;
            UpdateWeights();
        }
    }

    void UpdateInfo()
    {
        if (player == 0 && numGames[player].Value == 1)
        {
            SaveHeaderToFile();
        }
        SaveGameToFile();
        _gameManager.CurrentGameState = GameManager.GameState.Stopped;
        if (numGames[player].Value >= numGamesPerPlayer)
        {
            SaveStatisticsToFile();
            NextPlayer();
            ResetCounters();
            if (player < _gameManager.NumPlayers)
            {
                SaveHeaderToFile(false);
            }
        }
        if (player >= _gameManager.NumPlayers)
        {
            NextRound();
            for (int i = 0; i < numGames.Length; i++) { numGames[i] = new Counter(); }
        }
    }

    void UpdateWeights(int feature = 0)
    {
        if (feature > _gameManager.Minimax.NumEvalFeatures)
        {
            stopProgram = true;
        }
        else
        {
            weight[feature, 0] += weightInc[feature];
            if (weight[feature, 0] > weightMax[feature])
            {
                weight[feature, 0] = weightInit[feature];
                UpdateWeights(feature + 1);
            }
        }
    }
}
