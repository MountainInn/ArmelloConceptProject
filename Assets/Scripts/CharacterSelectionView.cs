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
        var characterCardPrefab = Resources.Load<CharacterCardView>("Prefabs/CharacterCard");
        var characterSOs = Resources.LoadAll<CharacterScriptableObject>("CharacterSOs");

        characterSOs
            .ToList()
            .ForEach(so =>
            {
                var newCard = GameObject.Instantiate(characterCardPrefab, Vector3.zero, Quaternion.identity, transform);

                newCard.SetScriptableObject(so);
                newCard.button.onClick.AddListener(()=> RadioSelect(newCard));
            });
    }

    void Start()
    {
        var firstCard = GetComponentInChildren<CharacterCardView>();
        RadioSelect(firstCard);
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
