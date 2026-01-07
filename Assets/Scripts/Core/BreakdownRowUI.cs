using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BreakdownRowUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text label;
    public TMP_Text value;
    // Optional bar
    public Image barFill;  // child image whose width we set (or use fillAmount on Image type Filled)

    public Color positiveColor = new Color(1f, 0.95f, 0.95f);
    public Color negativeColor = new Color(0.8f, 1f, 0.8f);

    public void Set(Sprite s, string labelText, string valueText, float normalized = -1f, float rawKg = 0f)
    {
        if (icon) icon.sprite = s;
        if (label) label.text = labelText;
        if (value) value.text = valueText;

        if (barFill)
        {
            // If using a horizontal "filled" image, set fillAmount (0..1).
            if (barFill.type == Image.Type.Filled && barFill.fillMethod == Image.FillMethod.Horizontal)
                barFill.fillAmount = Mathf.Clamp01(normalized);
            else
                barFill.rectTransform.localScale = new Vector3(Mathf.Clamp01(normalized), 1f, 1f);
        }

        if (value)
        {
            value.text = valueText;
            if (rawKg < 0f) value.color = new Color(0.15f, 0.7f, 0.3f); // greenish
            else value.color = Color.white;
        }
    
    }
}
