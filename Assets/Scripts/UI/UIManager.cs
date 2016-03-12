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
    public RectTransform ControlsPanel, InfoPanel, GamePanel, GameOverPanel;
    // Menu Panel
    public Toggle Player1Starts, Player1IsCPU, Player2IsCPU;
    public Slider DifficultySlider;
    public Image PlayerColor;
    public Text DifficultyLabel, InfoText;
    public RectTransform WallsPanel;
    public BarIntSlider WallsSlider;
    public Button PauseBtn, ResumeBtn, StartBtn;
    public Text WinnerInfo;
    public Image WinnerBackground;
    public bool PlayerClosedGameOverPanel;

    void Start()
    {
        DifficultySlider.wholeNumbers = true;
        DifficultySlider.minValue = 1;
        DifficultySlider.maxValue = Difficulties.Length;
        DifficultySlider.value = Mathf.Clamp(DifficultySlider.value, DifficultySlider.minValue, DifficultySlider.maxValue);
        DifficultySlider.value = GameBoard.Minimax.Depth;
        PlayerClosedGameOverPanel = false;
        WallsSlider.Size = GameBoard.NumWallsPerPlayer;
    }

    void Update()
    {
        PauseBtn.gameObject.SetActive(GameBoard.Ongoing);
        ResumeBtn.gameObject.SetActive(!GameBoard.Ongoing);

        Player1IsCPU.isOn = GameBoard.GetPlayer(0).IsCpu;
        Player2IsCPU.isOn = GameBoard.GetPlayer(1).IsCpu;
        DifficultySlider.value = GameBoard.Minimax.Depth;
        DifficultyLabel.text = string.Concat("Level [", Difficulties[GameBoard.Minimax.Depth - 1], "]");
        Player1Starts.isOn = (GameBoard.StartingPlayer == 0);

        if (GameBoard.Ongoing)
        {
            PlayerClosedGameOverPanel = false;
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

            WallsPanel.gameObject.SetActive(true);
            WallsSlider.BarColor = PlayerColor.color;
            WallsSlider.Value = GameBoard.CurrentPlayer.Walls;
        }
        else
        {
            PlayerColor.color = Color.clear;
            InfoText.text = "Game Not Running";
            if (GameBoard.IsGameOver)
            {
                GameOverPanel.gameObject.SetActive(!PlayerClosedGameOverPanel);
                if (GameBoard.Winner < 0)
                {
                    WinnerInfo.text = string.Concat("This time, a draw will do.");
                    PlayerColor.color = Color.clear;
                    WinnerBackground.color = PlayerColor.color;
                    InfoText.text = "Game Over: Draw";
                }
                else
                {
                    WinnerInfo.text = string.Concat("Player ", GameBoard.Winner + 1, ", you are the winner!");
                    PlayerColor.color = GameBoard.GetPlayer(GameBoard.Winner).Color;
                    WinnerBackground.color = PlayerColor.color;
                    InfoText.text = string.Concat("Game Over (Winner is player ", GameBoard.Winner + 1, ")");
                }
            }

            WallsPanel.gameObject.SetActive(false);
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

    public void OnPlayAgainClicked()
    {
        PlayerClosedGameOverPanel = true;
    }
}
