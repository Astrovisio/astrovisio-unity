using TMPro;
using UnityEngine;

public class AxesCanvasHandler : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI xCanvas;
    [SerializeField] private TextMeshProUGUI yCanvas;
    [SerializeField] private TextMeshProUGUI zCanvas;

    public void SetAxesLabel(string x, string y, string z)
    {
        xCanvas.text = x;
        yCanvas.text = y;
        zCanvas.text = z;
    }

}
