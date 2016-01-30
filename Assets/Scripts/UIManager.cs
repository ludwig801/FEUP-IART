using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
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

//    void Start()
//    {
//        _gameManager = GameManager.Instance;
//    }
//
//    void Update()
//    {
//        switch (_gameManager.CurrentGameState)
//        {
//            case GameManager.GameState.Stopped:
//                _gameManager.Players[0].IsCpu = Player1IsCPU.isOn;
//                _gameManager.Players[1].IsCpu = Player2IsCPU.isOn;
//                var sliderVal = (int)DifficultySlider.value;
//                _gameManager.Minimax.Depth = sliderVal + 1;
//                DifficultyLabel.text = Difficulties[sliderVal];
//                break;
//        }
//    }
//
//    public void OnNewGame()
//    {
//        _gameManager.InitialPlayer = Player1Starts ? 0 : 1;
//    }
}
