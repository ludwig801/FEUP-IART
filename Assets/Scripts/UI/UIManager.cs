using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameBoard GameBoard;
    public Debugger Debugger;

    public static string[] Difficulties = new string[]
    {
        "Normal",
        "Advanced",
        "Asian"
    };

    // Panels
    public RectTransform ControlsPanel, InfoPanel, GamePanel, DebugPanel, DebugPlayer0, DebugPlayer1;
    // Menu Panel
    public Toggle Player1Starts, Player1IsCPU, Player2IsCPU;
    public Slider DifficultySlider;
    public Image PlayerColor;
    public Text DifficultyLabel, InfoText;
    public Button PauseBtn, ResumeBtn, StartBtn;
    // Debug
    public Toggle DebugToggle;
    public Slider[] HeuristicsPlayer, HeuristicsOpponent;

    void Start()
    {
        DifficultySlider.wholeNumbers = true;
        DifficultySlider.minValue = 1;
        DifficultySlider.maxValue = Difficulties.Length;
        DifficultySlider.value = Mathf.Clamp(DifficultySlider.value, DifficultySlider.minValue, DifficultySlider.maxValue);
        DifficultySlider.value = GameBoard.Minimax.Depth;

        HeuristicsPlayer = DebugPlayer0.GetComponentsInChildren<Slider>();
        HeuristicsOpponent = DebugPlayer1.GetComponentsInChildren<Slider>();
    }

    void Update()
    {
        //ControlsPanel.gameObject.SetActive(!DebugToggle.isOn);
        //GamePanel.gameObject.SetActive(!DebugToggle.isOn);
        //InfoPanel.gameObject.SetActive(!DebugToggle.isOn);
        //DebugPanel.gameObject.SetActive(DebugToggle.isOn);

        PauseBtn.gameObject.SetActive(GameBoard.Ongoing);
        ResumeBtn.gameObject.SetActive(!GameBoard.Ongoing);
        //StartBtn.gameObject.SetActive(GameBoard.Ongoing);

        Player1IsCPU.isOn = GameBoard.GetPlayer(0).IsCpu;
        Player2IsCPU.isOn = GameBoard.GetPlayer(1).IsCpu;
        DifficultySlider.value = GameBoard.Minimax.Depth;
        DifficultyLabel.text = string.Concat("Level [", Difficulties[GameBoard.Minimax.Depth - 1], "]");
        Player1Starts.isOn = (GameBoard.StartingPlayer == 0);

        if (GameBoard.Ongoing)
        {
            if (GameBoard.IsGameOver)
            {
                if (GameBoard.Winner < 0)
                {
                    PlayerColor.color = Color.clear;
                    InfoText.text = "Game Over: Draw";
                }
                else
                {
                    PlayerColor.color = GameBoard.GetPlayer(GameBoard.Winner).Color;
                    InfoText.text = "Game Over";
                }
            }
            else
            {
                var currentPlayerIndex = GameBoard.CurrentPlayerIndex + 1;
                var currentPlayer = GameBoard.CurrentPlayer;
                PlayerColor.color = currentPlayer.Color;
                if (!currentPlayer.IsCpu)
                {
                    switch (GameBoard.CurrentMoveType)
                    {
                        case Move.Types.MovePawn:
                            InfoText.text = string.Concat("Player ", currentPlayerIndex, ": Move pawn");
                            break;

                        case Move.Types.PlaceWall:
                            InfoText.text = string.Concat("Player ", currentPlayerIndex, ": Place wall");
                            break;
                    }  
                }
                else
                {
                    InfoText.text = "Wait for CPU...";
                }
            }
        }
        else
        {
            PlayerColor.color = Color.clear;
            InfoText.text = "Game Not Running";
        }


        for (var i = 0; i < HeuristicsPlayer.Length; i++)
        {
            var slider = HeuristicsPlayer[i];
            slider.gameObject.SetActive(Debugger.HeuristicWeightsPlayer.Length > i);
            if (slider.gameObject.activeSelf)
            {
                slider.gameObject.SetActive(true);
                slider.minValue = Debugger.MinWeight;
                slider.maxValue = Debugger.MaxWeight;
                slider.value = Debugger.HeuristicWeightsPlayer[i];
            }
        }
        for (var i = 0; i < HeuristicsOpponent.Length; i++)
        {
            var slider = HeuristicsOpponent[i];
            slider.gameObject.SetActive(Debugger.HeuristicWeightsOpponent.Length > i);
            if (slider.gameObject.activeSelf)
            {
                slider.gameObject.SetActive(true);
                slider.minValue = Debugger.MinWeight;
                slider.maxValue = Debugger.MaxWeight;
                slider.value = Debugger.HeuristicWeightsOpponent[i];
            }
        }
    }

    public void OnValueChanged()
    {
        GameBoard.GetPlayer(0).IsCpu = Player1IsCPU.isOn;
        GameBoard.GetPlayer(1).IsCpu = Player2IsCPU.isOn;
        var sliderVal = (int)DifficultySlider.value;
        GameBoard.Minimax.Depth = sliderVal;
        DifficultyLabel.text = "Level: " + Difficulties[sliderVal - 1];
        GameBoard.StartingPlayer = Player1Starts.isOn ? 0 : 1;
    }
}
