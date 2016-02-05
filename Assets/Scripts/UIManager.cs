using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameBoard GameBoard;

    // Menu Panel
    public Toggle Player1Starts, Player1IsCPU, Player2IsCPU;
    public Slider DifficultySlider;
    public Text DifficultyLabel;
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
    }

    void OnGameStart()
    {
        GameBoard.CurrentPlayer = Player1Starts ? 0 : 1;
    }
}
