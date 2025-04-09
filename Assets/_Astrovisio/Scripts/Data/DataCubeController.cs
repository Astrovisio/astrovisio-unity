using UnityEngine;
using TMPro;

public class DataCubeController : MonoBehaviour
{
    [SerializeField] private DataCubeRenderer dataCubeRenderer;
    [SerializeField] private TMP_InputField inputThresholdMin;
    [SerializeField] private TMP_InputField inputThresholdMax;

    private void Start()
    {
        // Imposta i valori iniziali nei campi di input
        inputThresholdMin.text = dataCubeRenderer.thresholdMin1.ToString("F2");
        inputThresholdMax.text = dataCubeRenderer.thresholdMax1.ToString("F2");

        // Aggiunge listener per rilevare quando l'utente modifica il valore
        inputThresholdMin.onEndEdit.AddListener(UpdateThresholdMin);
        inputThresholdMax.onEndEdit.AddListener(UpdateThresholdMax);
    }

    public void UpdateThresholdMin(string newValue)
    {
        if (float.TryParse(newValue, out float value))
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f); // Limita il valore tra 0 e 1
            dataCubeRenderer.thresholdMin1 = value;
            inputThresholdMin.text = value.ToString("F2"); // Aggiorna UI con valore formattato
        }
    }

    public void UpdateThresholdMax(string newValue)
    {
        if (float.TryParse(newValue, out float value))
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            dataCubeRenderer.thresholdMax1 = value;
            inputThresholdMax.text = value.ToString("F2");
        }
    }
}
