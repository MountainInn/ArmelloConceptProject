using UnityEngine;
using TMPro;
using static TMPro.TMP_Dropdown;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;

public class PlayerCustomizationView : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_Dropdown colorDropdown;

    public string playerName { get; private set; }
    public Color playerColor { get; private set; }

    public struct MsgNameChanged{ public string name; }
    public struct MsgColorChanged{ public Color color; }

    private void Awake()
    {
        colorDropdown.options =
            System.Enum
            .GetNames(typeof(PlayerColors))
            .Select(colorName => new OptionData(colorName))
            .ToList();
    }

    private void Start()
    {
        MessageBroker.Default.Publish(new MsgObjectStarted<PlayerCustomizationView>(this));

        colorDropdown.onValueChanged.AddListener(OnColorChanged);
        colorDropdown.value = 0;
        colorDropdown.onValueChanged.Invoke(0);

        nameInputField.onValueChanged.AddListener(OnNameChanged);
        nameInputField.text = PlayerPrefs.GetString("Nickname");
        nameInputField.onValueChanged.Invoke(nameInputField.text);
    }

    private void OnNameChanged(string newName)
    {
        playerName = newName;
        PlayerPrefs.SetString("Nickname", playerName);

        MessageBroker.Default.Publish(new MsgNameChanged{ name = newName });
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

        MessageBroker.Default.Publish(new MsgColorChanged{ color = playerColor });
    }

    public enum PlayerColors
        {
            Red, Blue, Green, Yellow
        }
}
