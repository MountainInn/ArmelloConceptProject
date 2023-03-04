using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Zenject;
using System;

public class CharacterSelectionView : MonoBehaviour
{
    [SerializeField] RectTransform content;

    CharacterCardView[] characterCards;
    CharacterCardView selected;

    ColorBlock defaultColorBlock;
    public event Action<CharacterScriptableObject> onSelectedCharacterChanged;

    void Awake()
    {
        var characterCardPrefab = Resources.Load<CharacterCardView>("Prefabs/Character Card");
        var characterSOs = Resources.LoadAll<CharacterScriptableObject>("Characters");

        characterSOs
            .ToList()
            .ForEach(so =>
            {
                var newCard = GameObject.Instantiate(characterCardPrefab, Vector3.zero, Quaternion.identity, content);

                newCard.transform.localEulerAngles = Vector3.zero;
                newCard.transform.localPosition = Vector3.zero;
                newCard.transform.localScale = Vector3.one;

                newCard.SetScriptableObject(so);
                newCard.button.onClick.AddListener(()=> RadioSelect(newCard));
            });
    }

    void Start()
    {
        var firstCard = GetComponentInChildren<CharacterCardView>();

        defaultColorBlock = firstCard.button.colors;

        RadioSelect(firstCard);
    }

    void RadioSelect(CharacterCardView card)
    {
        if (selected == card)
            return;

        ColorBlock colorBlock = defaultColorBlock;

        if (selected != null)
        {
            selected.button.colors = colorBlock;
        }
       
        selected = card;

        colorBlock.normalColor = Color.Lerp(colorBlock.normalColor, Color.green, .3f);
        colorBlock.pressedColor = Color.Lerp(colorBlock.pressedColor, Color.green, .3f);
        colorBlock.disabledColor = Color.Lerp(colorBlock.disabledColor, Color.green, .3f);
        colorBlock.selectedColor = Color.Lerp(colorBlock.selectedColor, Color.green, .3f);
        colorBlock.highlightedColor = Color.Lerp(colorBlock.highlightedColor, Color.green, .3f);

        selected.button.colors = colorBlock;

        onSelectedCharacterChanged?.Invoke(GetSelectedCharacter());
    }

    public CharacterScriptableObject GetSelectedCharacter()
    {
        return selected.GetCharacterScriptableObject();
    }
}
