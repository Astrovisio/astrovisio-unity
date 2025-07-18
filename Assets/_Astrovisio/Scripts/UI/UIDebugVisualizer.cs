using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIDebugVisualizer : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private bool _debugActive = true;

    private VisualElement root;
    private VisualElement mouseMarker;
    private VisualElement pickHighlight;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("uiDocument not assigned");
            return;
        }

        root = uiDocument.rootVisualElement;

        // Red
        mouseMarker = new VisualElement();
        mouseMarker.style.width = 16;
        mouseMarker.style.height = 16;
        mouseMarker.style.backgroundColor = new Color(1f, 0f, 0f, 0.8f);
        mouseMarker.style.position = Position.Absolute;
        mouseMarker.pickingMode = PickingMode.Ignore;
        root.Add(mouseMarker);

        // Yellow
        pickHighlight = new VisualElement();
        pickHighlight.style.borderTopWidth = 2;
        pickHighlight.style.borderBottomWidth = 2;
        pickHighlight.style.borderLeftWidth = 2;
        pickHighlight.style.borderRightWidth = 2;

        pickHighlight.style.borderTopColor = Color.yellow;
        pickHighlight.style.borderBottomColor = Color.yellow;
        pickHighlight.style.borderLeftColor = Color.yellow;
        pickHighlight.style.borderRightColor = Color.yellow;

        pickHighlight.style.position = Position.Absolute;
        pickHighlight.pickingMode = PickingMode.Ignore;
        root.Add(pickHighlight);

        UpdateDebugVisibility();
    }

    private void Update()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.y = Screen.height - mouseScreenPos.y;

        if (root?.panel != null)
        {
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(root.panel, mouseScreenPos);
            mouseMarker.style.left = panelPos.x - 4;
            mouseMarker.style.top = panelPos.y - 4;

            if (_debugActive)
            {
                VisualElement picked = root.panel.Pick(panelPos);

                if (picked != null && picked != root)
                {
                    var bounds = picked.worldBound;
                    pickHighlight.style.left = bounds.x;
                    pickHighlight.style.top = bounds.y;
                    pickHighlight.style.width = bounds.width;
                    pickHighlight.style.height = bounds.height;
                    pickHighlight.style.display = DisplayStyle.Flex;
                }
                else
                {
                    pickHighlight.style.display = DisplayStyle.None;
                }
            }
        }

        UpdateDebugVisibility();
    }


    private void UpdateDebugVisibility()
    {
        DisplayStyle visibility = _debugActive ? DisplayStyle.Flex : DisplayStyle.None;
        if (mouseMarker != null) mouseMarker.style.display = visibility;
        if (pickHighlight != null) pickHighlight.style.display = visibility;
    }

}
