using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    GameManager _gameManager;

    // Menu Panel
    public Toggle toggleCPU_0, toggleCPU_1;
    public Slider startingPlayerSlider;
    public Button startGameBtn, pauseGameBtn, stopGameBtn;
    public Toggle testMode;

    // Game Panel
    public Text turnText, actionText, walls0Text, walls1Text;
    public Button placeWallH, placeWallV;
    public Slider minimaxDepthSlider;
    public Text minimaxDepthValue;
    public GameObject gameOverPanel;
    public Text gameOverText;

    public Slider[] weight0, weight1;
    public Button applyWeights, saveWeights;

    public void Start()
    {
        _gameManager = GameManager.Instance;

//        pauseGameBtn.interactable = false;
//        stopGameBtn.interactable = false;
//
//        startingPlayerValue = startingPlayerSlider.transform.Find(NamesUI.Value).GetComponent<Text>();
//        startingPlayerValue.text = "" + startingPlayerSlider.value;
//
//        _manager.CPU_0 = toggleCPU_0.isOn;
//        _manager.CPU_1 = toggleCPU_1.isOn;
//
//        turnText.text = "Turn: none";
//        actionText.text = "Action: none";
//        placeWallH.interactable = false;
//        placeWallV.interactable = false;
//        minimaxDepthSlider.interactable = true;
//
//        walls0Text.text = "Walls [0]: " + _manager.availableWalls[0];
//        walls1Text.text = "Walls [1]: " + _manager.availableWalls[1];
//
//        minimaxDepthValue.text = "" + _manager.minimaxDepth;
//
//        for (int i = 0; i < weight0.Length; i++)
//        {
//            weight0[i].value = _manager.GetWeight(0, i);
//        }
//
//        for (int i = 0; i < weight1.Length; i++)
//        {
//            weight1[i].value = _manager.GetWeight(1, i);
//        }
//
//        _manager.testMode = testMode.isOn;
//
//        gameOverPanel.SetActive(false);
    }

    public void Update()
    {
//        turnText.text = "Turn: " + _manager.currentPlayer;
//        walls0Text.text = "Walls [0]: " + _manager.availableWalls[0];
//        walls1Text.text = "Walls [1]: " + _manager.availableWalls[1];
//
//        if (_manager.gameState == GameManager.GameState.Error)
//        {
//            toggleCPU_0.interactable = false;
//            toggleCPU_1.interactable = false;
//            startingPlayerSlider.interactable = false;
//            startGameBtn.interactable = false;
//            pauseGameBtn.interactable = false;
//            stopGameBtn.interactable = false;
//            testMode.interactable = false;
//            placeWallH.interactable = false;
//            placeWallV.interactable = false;
//            minimaxDepthSlider.interactable = false;      
//        }
//        else if (_manager.gameState == GameManager.GameState.Over)
//        {
//            gameOverPanel.SetActive(true);
//            gameOverText.text = "Winner: " + _manager.winner;
//            toggleCPU_0.interactable = false;
//            toggleCPU_1.interactable = false;
//            startingPlayerSlider.interactable = false;
//            startGameBtn.interactable = false;
//            pauseGameBtn.interactable = false;
//            stopGameBtn.interactable = false;
//            testMode.interactable = false;
//            placeWallH.interactable = false;
//            placeWallV.interactable = false;
//            minimaxDepthSlider.interactable = false;
//            for (int i = 0; i < weight0.Length; i++)
//            {
//                weight0[i].interactable = false;
//            }
//
//            for (int i = 0; i < weight1.Length; i++)
//            {
//                weight1[i].interactable = false;
//            }
//            applyWeights.interactable = false;
//            saveWeights.interactable = false;
//        }
//        else if (_manager.gameState == GameManager.GameState.Paused)
//        {
//            toggleCPU_0.interactable = true;
//            toggleCPU_1.interactable = true;
//            startingPlayerSlider.interactable = false;
//            startGameBtn.interactable = false;
//            pauseGameBtn.interactable = true;
//            stopGameBtn.interactable = true;
//            testMode.interactable = true;
//            placeWallH.interactable = false;
//            placeWallV.interactable = false;
//            minimaxDepthSlider.interactable = true;
//        }
//        else if (_manager.gameState == GameManager.GameState.Stopped)
//        {
//            toggleCPU_0.interactable = true;
//            toggleCPU_1.interactable = true;
//            startingPlayerSlider.interactable = true;
//            startGameBtn.interactable = true;
//            pauseGameBtn.interactable = false;
//            stopGameBtn.interactable = false;
//            testMode.interactable = true;
//            placeWallH.interactable = false;
//            placeWallV.interactable = false;
//            minimaxDepthSlider.interactable = true;
//        }
//        else if (_manager.gameState == GameManager.GameState.Ongoing)
//        {
//            toggleCPU_0.interactable = false;
//            toggleCPU_1.interactable = false;
//            startingPlayerSlider.interactable = false;
//            if (_manager.IsCPUCurrentPlayer())
//            {
//                placeWallH.interactable = false;
//                placeWallV.interactable = false;
//            }
//            else
//            {
//                placeWallH.interactable = true;
//                placeWallV.interactable = true;
//            }
//
//            if (_manager.mode == GameManager.Mode.MovePawn)
//            {
//                placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Horizontal Wall";
//                placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Vertical Wall";
//                actionText.text = "Action: Move Pawn";
//            }
//            else if (_manager.mode == GameManager.Mode.PlaceWallH)
//            {
//                placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Vertical Wall";
//                placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Cancel Placement";
//                actionText.text = "Action: Place Horizontal Wall";
//            }
//            else if (_manager.mode == GameManager.Mode.PlaceWallV)
//            {
//                placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Horizontal Wall";
//                placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Cancel Placement";
//                actionText.text = "Action: Place Vertical Wall";
//            }
//            else if (_manager.mode == GameManager.Mode.None)
//            {
//                placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Horizontal Wall";
//                placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Vertical Wall";
//                actionText.text = "Action: None";
//            }
//        }
    }

    public void OnToggleCPU0Pressed(bool val)
    {
        //Debug.Log("CPU 0: " + toggleCPU_0.isOn);
        _gameManager.CPU_0 = toggleCPU_0.isOn;
    }

    public void OnToggleCPU1Pressed(bool val)
    {
        //Debug.Log("CPU 1: " + toggleCPU_1.isOn);
        _gameManager.CPU_1 = toggleCPU_1.isOn;
    }

    public void OnStartGameButtonPressed()
    {
        //Debug.Log("Start Game");
        _gameManager.NewGame((int)startingPlayerSlider.value);
        startGameBtn.interactable = false;
        stopGameBtn.interactable = true;
        pauseGameBtn.interactable = true;

        turnText.text = "Turn: " + _gameManager.CurrentPlayer;
        actionText.text = "Action: none";
        placeWallH.interactable = true;
        placeWallV.interactable = true;
        minimaxDepthSlider.interactable = true;
    }

    public void OnStopGameButtonPressed()
    {
        //Debug.Log("Reset Game");
        _gameManager.CurrentGameState = GameManager.GameState.Stopped;
        startGameBtn.interactable = true;
        stopGameBtn.interactable = false;
        pauseGameBtn.interactable = false;
    }

    public void OnPauseGameButtonPressed()
    {
        //Debug.Log("Pause Game");
        if (_gameManager.CurrentGameState == GameManager.GameState.Paused)
        {
            _gameManager.CurrentGameState = GameManager.GameState.Ongoing;
            pauseGameBtn.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Pause Game";
        }
        else
        {
            _gameManager.CurrentGameState = GameManager.GameState.Paused;
            pauseGameBtn.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Resume Game";
        }
    }

    public void OnTestModeChanged()
    {
        _gameManager.testMode = testMode.isOn;
    }

    public void OnPlaceWallHorizontalPressed()
    {
        if (_gameManager.CurrentGameMode == GameManager.GameMode.PlaceWallH)
        {
            placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Horizontal Wall";
            _gameManager.CurrentGameMode = GameManager.GameMode.None;
        }
        else
        {
            placeWallH.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Cancel Placement";
            _gameManager.CurrentGameMode = GameManager.GameMode.PlaceWallH;
        }
    }

    public void OnPlaceWallVerticalPressed()
    {
        if (_gameManager.CurrentGameMode == GameManager.GameMode.PlaceWallV)
        {
            placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Place Vertical Wall";
            _gameManager.ChangeModeTo(GameManager.GameMode.None);
        }
        else
        {
            placeWallV.transform.Find(NamesUI.Text).GetComponent<Text>().text = "Cancel Placement";
            _gameManager.ChangeModeTo(GameManager.GameMode.PlaceWallV);
        }
    }

    public void OnMinimaxDepthChanged()
    {
        _gameManager.minimaxDepth = (int)minimaxDepthSlider.value;
        minimaxDepthValue.text = "" + _gameManager.minimaxDepth;
    }

    public void ApplyWeights()
    {
        for (int i = 0; i < weight0.Length; i++)
        {
            _gameManager.Minimax.SetWeight(0, i, weight0[i].value);
        }

        for (int i = 0; i < weight1.Length; i++)
        {
            _gameManager.Minimax.SetWeight(1, i, weight1[i].value);
        }

        //manager.PrintWeights();
    }

    public void OnGoAgainPressed()
    {
        _gameManager.CurrentGameState = GameManager.GameState.Stopped;
        gameOverPanel.SetActive(false);
        gameOverText.text = "Winner: NONE";
        for (int i = 0; i < weight0.Length; i++)
        {
            weight0[i].interactable = true;
        }

        for (int i = 0; i < weight1.Length; i++)
        {
            weight1[i].interactable = true;
        }
        applyWeights.interactable = true;
        saveWeights.interactable = true;
    }

    public void OnUndoPressed()
    {
        _gameManager.UndoMove();
    }
}
