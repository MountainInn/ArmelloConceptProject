using UnityEngine;
using TMPro;
using static TMPro.TMP_Dropdown;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerCustomizationView : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_Dropdown colorDropdown;

    public string playerName;
    public Color playerColor;

    private void Awake()
    {
        colorDropdown.options =
            System.Enum
            .GetNames(typeof(PlayerColors))
            .Select(colorName => new OptionData(colorName))
            .ToList();

        colorDropdown.onValueChanged.AddListener(OnColorChanged);

        nameInputField.onValueChanged.AddListener(OnNameChanged);

        colorDropdown.value = 0;
        colorDropdown.onValueChanged.Invoke(0);
    }

    private void OnNameChanged(string newName)
    {
        playerName = newName;
    }

    private void OnColorChanged(int optionId)
    {
        var colorEnum = (PlayerColors)optionId;

        playerColor = colorEnum switch {
            (PlayerColors.Red) => Color.red,
            (PlayerColors.Blue) => Color.blue,
            (PlayerColors.Green) => Color.green,
            (PlayerColors.Yellow) => Color.yellow,
            (_) => throw new System.Exception("Not all color options handled in switch block.")
        };
    }

    public enum PlayerColors
        {
            Red, Blue, Green, Yellow
        }
}
