using System;
using TMPro;
using UnityEngine;

public class ResourcesView : MonoBehaviour
{
    public TextMeshProUGUI actionPointLabel;
    public TextMeshProUGUI movementPointLabel;

    public void UpdateActionPoints(int actionPoints)
    {
        actionPointLabel.text = $"Action Points: {actionPoints}";
    }

    internal void UpdateMovementPoints(int movementPoints)
    {
        movementPointLabel.text = $"Move Points: {movementPoints}";
    }
}
