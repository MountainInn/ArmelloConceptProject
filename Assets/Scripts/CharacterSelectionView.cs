using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterSelectionView : MonoBehaviour
{
    CharacterCardView[] characterCards;
    CharacterCardView selected;

    void Awake()
    {
        characterCards = GetComponentsInChildren<CharacterCardView>();
        characterCards
            .Select(card => (card, card.button))
            .ToList()
            .ForEach(tup => tup.button.onClick.AddListener(()=> RadioSelect(tup.card)));
    }

    void RadioSelect(CharacterCardView card)
    {
        ColorBlock colorBlock = default;

        if (selected != null)
        {
            colorBlock = selected.button.colors;
            colorBlock.normalColor = Color.white;
            selected.button.colors = colorBlock;
        }
       
        selected = card;

        colorBlock.normalColor = Color.Lerp(Color.white, Color.green, .1f);
        selected.button.colors = colorBlock;
    }

    public CharacterScriptableObject GetSelectedCharacter()
    {
        if (selected == null)
            throw new System.NullReferenceException("CharacterSelectionView.selected is null.");

        return selected.GetCharacterScriptableObject();
    }
}
