using UnityEngine;

[CreateAssetMenu(fileName = "UIColorSO", menuName = "Scriptable Objects/UIColorSO")]
public class UIColorSO : ScriptableObject
{

    [Header("Colors")]
    public Color accent100 = new Color(1f, 0.4f, 0.11f, 1f);
    public Color accent200 = new Color(0.84f, 0.25f, 0f, 1f);
    public Color white = new Color(1f, 1f, 1f, 1f);
    public Color black = new Color(0f, 0f, 0f, 1f);
    public Color transparent = new Color(1f, 1f, 1f, 0f);
    public Color bg10 = new Color(1f, 1f, 1f, 0.1f);
    public Color success = new Color(0.05f, 0.32f, 0.13f, 1f);
    public Color error = new Color(1f, 0.2f, 0.2f, 1f);
    public ColorContainer lightColorContainer;
    public ColorContainer darkColorContainer;

}

public enum ColorMode {
    Light,
    Dark
}

[System.Serializable]
public class ColorContainer
{
    public Color grey100;
    public Color grey150;
    public Color grey200;
    public Color grey300;
    public Color grey400;
    public Color grey450;
    public Color grey500;
    public Color grey600;
    public Color grey700;
    public Color grey800;
}