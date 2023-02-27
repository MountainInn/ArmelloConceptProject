using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using MountainInn;
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
                newText.transform.localEulerAngles = Vector3.zero;
                newText.transform.localPosition = Vector3.zero;

                resourceTexts.Add(r, newText);

                SetResource(r, 0);
            });
    }

    public void SetResourcesSync(SyncIDictionary<ResourceType, int> resources)
    {
        resources.Callback += OnResourcesSync;
    }

    private void OnResourcesSync(SyncIDictionary<ResourceType, int>.Operation op, ResourceType key, int item)
    {
        switch (op)
        {
            case SyncIDictionary<ResourceType, int>.Operation.OP_ADD:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_SET:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_REMOVE:
                break;
            case SyncIDictionary<ResourceType, int>.Operation.OP_CLEAR:
                break;
        }

        SetResource(key, item);
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
