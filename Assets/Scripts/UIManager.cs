using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameBoard GameBoard;

    // Menu Panel
    public Toggle Player1Starts, Player1IsCPU, Player2IsCPU;
    public Slider DifficultySlider;
    public Image PlayerColor;
    public Text DifficultyLabel, Player, InfoText;
    public string[] Difficulties = new string[]
    {
        "Normal",
        "Advanced",
        "Asian"
    };

    void Start()
    {
        DifficultySlider.wholeNumbers = true;
        DifficultySlider.minValue = 0;
        DifficultySlider.value = 0;
        DifficultySlider.maxValue = Difficulties.Length - 1;
    }

    void Update()
    {
        GameBoard.Players[0].IsCpu = Player1IsCPU.isOn;
        GameBoard.Players[1].IsCpu = Player2IsCPU.isOn;
        var sliderVal = (int)DifficultySlider.value;
        GameBoard.Minimax.Depth = sliderVal + 1;
        DifficultyLabel.text = "Level: " + Difficulties[sliderVal];
        GameBoard.StartingPlayer = Player1Starts.isOn ? 0 : 1;

        if (GameBoard.Ongoing)
        {
            var CurrentPlayer = GameBoard.Players[GameBoard.CurrentPlayer];
            PlayerColor.color = CurrentPlayer.Color;
            Player.text = "Player " + (GameBoard.CurrentPlayer + 1) + (CurrentPlayer.IsCpu ? " [CPU]" : "");
            if (!CurrentPlayer.IsCpu)
            {
                switch (GameBoard.MoveType)
                {
                    case Move.Types.MovePawn:
                        InfoText.text = "Move your pawn...";
                        break;

                    case Move.Types.PlaceWall:
                        InfoText.text = "Place your wall...";
                        break;
                }  
            }
            else
            {
                InfoText.text = "CPU move...";
            }
        }
        else
        {
            PlayerColor.color = Color.white;
            Player.text = "No Player Selected";
            InfoText.text = "";   
        }
    }
}
