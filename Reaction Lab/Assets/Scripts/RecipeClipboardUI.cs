using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class RecipeClipboardUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI textDisplay;

    private List<string> lines = new List<string>();

    // Now includes the machine name
    public void AddReactionToClipboard(string ingredientA, string ingredientB, string result, string machineName)
    {
        string line;

        if (string.IsNullOrEmpty(ingredientB) || ingredientB == "None")
            line = $"{ingredientA} = {result} ({machineName})";
        else
            line = $"{ingredientA} + {ingredientB} = {result} ({machineName})";

        lines.Add(line);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        StringBuilder sb = new StringBuilder();
        foreach (string line in lines)
        {
            sb.AppendLine(line);
        }
        textDisplay.text = sb.ToString();
    }
}
