using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{

    // Deprecated ?
    public class DataInspectorController
    {
        public VisualElement Root { get; }

        private ScrollView paramScrollView;

        public DataInspectorController(VisualElement root)
        {
            Root = root;

            paramScrollView = Root.Q<ScrollView>("ParamScrollView");
            // Debug.Log(paramScrollView);

            SetVisibility(false);
        }

        public void SetVisibility(bool visibility)
        {
            if (visibility)
            {
                Root.style.display = DisplayStyle.Flex;
            }
            else
            {
                Root.style.display = DisplayStyle.None;
            }
        }

        public void SetData(string[] header, float[] dataInfo)
        {
            paramScrollView.Clear();

            for (int i = 0; i < dataInfo.Length; i++)
            {
                AddParamRow(header[i] + ": " + dataInfo[i]);
            }
        }

        private void AddParamRow(string text)
        {
            Label label = new Label();
            label.text = text;
            label.AddToClassList("param-row");
            paramScrollView.Add(label);
            // Debug.Log(text);
        }

    }

}