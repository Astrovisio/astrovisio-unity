using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DataInspector : MonoBehaviour
{

    [Header("Fresenel")]
    [SerializeField] private Color fresnelColor = Color.white;
    [SerializeField][Range(0.1f, 2f)] private float fresnelPower = 1f;

    public bool IsActive { private set; get; }

    private MeshRenderer meshRenderer;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        SetFresnelColor(fresnelColor);
        SetFresnelPower(fresnelPower);
    }

    public void SetActiveState(bool state)
    {
        meshRenderer.enabled = state;
        IsActive = state;
    }

    public void SetFresnelColor(Color newColor)
    {
        meshRenderer.material.SetColor("_FresnelColor", newColor);
    }

    public void SetFresnelPower(float newPower)
    {
        meshRenderer.material.SetFloat("_FresnelPower", newPower);
    }

    public void SetScale(float newScale)
    {
        transform.localScale = Vector3.one * newScale;
    }

}
