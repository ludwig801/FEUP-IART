using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class BarIntSlider : MonoBehaviour
{
    public RectTransform SliderRect;
    public RectTransform BarItemPrefab;
    public int Padding;
    public int SpacingFactor;
    public Vector2 ItemProportion;
    public Sprite BarSprite;
    public bool PreserveAspect;
    [SerializeField]
    Color _barColor;
    [SerializeField]
    int _size;
    [SerializeField]
    int _value;

    public int Size
    {
        get
        {
            return _size;
        }

        set
        {
            if (value > _bars.Count)
            {
                for (int i = _bars.Count; i < value; i++)
                {
                    AddBar();
                }
            }
            else if (value < _bars.Count)
            {
                _bars.RemoveRange(value - 1, _size - value);
            }

            _size = _bars.Count;
            // So that the value gets clamped
            Value = Value;
        }
    }

    public int Value
    {
        get
        {
            return _value;
        }

        set
        {
            value = Mathf.Clamp(value, 0, Size);
            for (int i = _bars.Count - 1; i >= 0; i--)
            {
                _bars[i].enabled = (value > i);
            }
            _value = value;
        }
    }

    public Color BarColor
    {
        get
        {
            return _barColor;
        }

        set
        {
            _barColor = value;
            foreach (var img in _bars)
            {
                img.color = value;
            }
        }
    }

    [SerializeField]
    private List<Image> _bars;

    void Start()
    {
        var content = SliderRect.gameObject.AddComponent<ContentSizeFitter>();
        content.horizontalFit = ContentSizeFitter.FitMode.MinSize;
        content.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        var layout = SliderRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding.left = Padding;
        layout.padding.right = Padding;
        layout.padding.top = Padding;
        layout.padding.bottom = Padding;
        layout.spacing = SpacingFactor * Padding;
        layout.childForceExpandWidth = false;

        _bars = new List<Image>();
    }

    void AddBar()
    {
        var item = Instantiate(BarItemPrefab);
        item.name = "Bar";
        item.SetParent(SliderRect);
        var img = item.GetComponent<Image>();
        img.sprite = BarSprite;
        img.color = _barColor;
        img.type = Image.Type.Simple;
        img.preserveAspect = PreserveAspect;
        var layout = img.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = (ItemProportion.y * SliderRect.rect.height) - (2 * Padding);
        layout.minWidth = (ItemProportion.x * SliderRect.rect.width - 2 * Padding - (Size - 1) * SpacingFactor * Padding) / Size;
        _bars.Add(img);
    }
}
