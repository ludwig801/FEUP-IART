using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SliderValue : MonoBehaviour
{
    public Slider Slider;
    [SerializeField]
    Text _text;

    void Start()
    {
        _text = GetComponent<Text>();
    }

    void Update()
    {
        _text.text = Slider.value.ToString();
    }
}
