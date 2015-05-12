using UnityEngine;
using System.Collections;
using System.IO;

[RequireComponent(typeof(GameManager))]
public class Tester : MonoBehaviour
{
    public int numGamesPerPlayer;

    bool ongoingRounds;
    bool stopProgram;
    GameManager manager;

    // Statistical variables
    int currentFeature;
    float[,] weight;
    float[] weightInc;
    float[] weightMax;
    Counter numGames;
    Counter numRounds;
    Counter wins;
    Counter winsTotal;
    Counter losses;
    Counter draws;

    // Use this for initialization
    void Start()
    {
        manager = GetComponent<GameManager>();
        InitializeVars();
    }

    void InitializeVars()
    {
        ongoingRounds = false;
        numGames = new Counter();
        numRounds = new Counter();
        wins = new Counter();
        winsTotal = new Counter();
        losses = new Counter();
        draws = new Counter();

        weight = new float[GameManager.NumEvalFeatures, GameManager.NumPlayers];
        weightInc = new float[GameManager.NumEvalFeatures];
        weightMax = new float[GameManager.NumEvalFeatures];

        weightInc[0] = 0.2f;
        weightInc[1] = 0.2f;
        weightInc[2] = 0.1f;
        weightInc[3] = 0.1f;
        currentFeature = 0;

        weightMax[0] = 3f;
        weightMax[1] = 3f;
        weightMax[2] = 1f;
        weightMax[3] = 1f;

        weight[0, 0] = 0.2f;
        weight[1, 0] = 0.2f;
        weight[2, 0] = 0f;
        weight[3, 0] = 0f;

        weight[0, 1] = 1f;
        weight[1, 1] = 1f;
        weight[2, 1] = 1f;
        weight[3, 1] = 1f;

        stopProgram = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ongoingRounds)
        {
            if (!stopProgram)
            {
                switch (manager.gameState)
                {
                    case GameManager.GameState.Ongoing:
                        break;
                    case GameManager.GameState.Error:
                        manager.Reset();
                        numGames.Inc();
                        break;
                    case GameManager.GameState.Stopped:
                        if (numGames.Value < numGamesPerPlayer)
                        {
                            manager.NewGame(0);
                        }
                        else if (numGames.Value < (numGamesPerPlayer * 2))
                        {
                            manager.NewGame(1);
                        }
                        else
                        {
                            numRounds.Inc();
                        }
                        break;
                    case GameManager.GameState.Draw:
                        numGames.Inc();
                        draws.Inc();
                        break;
                    case GameManager.GameState.Over:
                        if (manager.winner == 0)
                        {
                            wins.Inc();
                            winsTotal.Inc();
                        }
                        else
                        {
                            losses.Inc();
                        }
                        if (numGames.Value == 0)
                        {
                            SaveHeaderToFile(0);
                            SaveStatisticsToFile(0);
                        }
                        else if (numGames.Value < numGamesPerPlayer)
                        {
                            SaveStatisticsToFile(0);
                        }
                        else if (numGames.Value == numGamesPerPlayer)
                        {
                            ResetCounters();
                            SaveHeaderToFile(1);
                            SaveStatisticsToFile(1);
                        }
                        else if (numGames.Value < (numGamesPerPlayer * 2))
                        {
                            SaveStatisticsToFile(1);
                        }
                        else
                        {
                            UpdateWeights();
                            ResetCounters();
                            winsTotal.Reset();
                        }
                        numGames.Inc();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
            }
        }
    }

    void SaveHeaderToFile(int startingPlayer)
    {
        StreamWriter stream = new StreamWriter(Names.SavePath_ + numRounds.Value, true);
        stream.WriteLine("--------------------------" + numRounds.Value);
        stream.WriteLine("Round: " + numRounds.Value);
        stream.WriteLine("Num Games Per Player: " + numGamesPerPlayer);
        stream.WriteLine("--------------------------" + numRounds.Value);
        stream.WriteLine("Feature 1: " + weight[0, 0]);
        stream.WriteLine("Feature 2: " + weight[1, 0]);
        stream.WriteLine("Feature 3: " + weight[2, 0]);
        stream.WriteLine("Feature 4: " + weight[3, 0]);
        stream.Close();
    }

    void SaveStatisticsToFile(int startingPlayer)
    {
        StreamWriter stream = new StreamWriter(Names.SavePath_ + numRounds.Value, true);
        stream.WriteLine("--------------------------" + numRounds.Value);
        stream.WriteLine("Game: " + numGames.Value);
        stream.WriteLine("Wins 1: " + wins.Value);
        stream.WriteLine("Wins 2: " + losses.Value);
        stream.WriteLine("Draws:" + draws.Value);
        stream.Close();
    }

    void ResetCounters()
    {
        wins.Reset();
        losses.Reset();
        draws.Reset();
        numGames.Reset();
    }

    void UpdateWeights()
    {
        if (weight[currentFeature, 0] < weightMax[currentFeature])
        {
            weight[currentFeature, 0] += weightInc[currentFeature];
        }
        else
        {
            currentFeature++;
            if (currentFeature >= GameManager.NumEvalFeatures)
            {
                stopProgram = true;
            }
            else
            {
                weight[currentFeature, 0] += weightInc[currentFeature];
            }
        }
    }

    public void StartRounds()
    {
        ongoingRounds = true;
    }

    public void StopRounds()
    {
        ongoingRounds = false;
    }
}
