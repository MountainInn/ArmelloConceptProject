using System;
using TMPro;

public class ResourcesView
{
    public TextMeshProUGUI actionPointLabel;

    public void UpdateActionPoints(int actionPoints)
    {
        actionPointLabel.text = $"AP: {actionPoints}";
    }
}
