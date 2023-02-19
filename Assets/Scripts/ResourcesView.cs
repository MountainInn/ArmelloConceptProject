using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResourcesView : MonoBehaviour
{
    public TextMeshProUGUI actionPointLabel;
    public TextMeshProUGUI movementPointLabel;

    Dictionary<ResourceType, TextMeshProUGUI> resourceTexts = new Dictionary<ResourceType, TextMeshProUGUI>();


    private void Awake()
    {
        System.Enum.GetValues(typeof(ResourceType))
            .Cast<ResourceType>()
            .ToList()
            .ForEach(r =>
            {
                var newText = GameObject.Instantiate(actionPointLabel, Vector3.zero, Quaternion.identity, transform);
                resourceTexts.Add(r, newText);
            });
    }

    public void UpdateActionPoints(int actionPoints)
    {
        actionPointLabel.text = $"Action Points: {actionPoints}";
    }

    internal void UpdateMovementPoints(int movementPoints)
    {
        movementPointLabel.text = $"Move Points: {movementPoints}";
    }

    internal void SetResource(ResourceType key, int item)
    {
        resourceTexts[key].text = $"{key}: {item}";
    }
}
