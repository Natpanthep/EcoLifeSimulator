using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text label;

    public void Set(Sprite sprite, string text)
    {
        if (icon)  icon.sprite = sprite;
        if (label) label.text  = text;
    }
}
