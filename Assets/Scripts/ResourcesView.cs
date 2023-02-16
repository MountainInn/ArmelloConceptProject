using System;
using TMPro;
using UnityEngine;

public class ResourcesView : MonoBehaviour
{
    public TextMeshProUGUI actionPointLabel;

    public void UpdateActionPoints(int actionPoints)
    {
        actionPointLabel.text = $"AP: {actionPoints}";
    }
}
