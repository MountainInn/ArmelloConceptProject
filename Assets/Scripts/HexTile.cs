using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class HexTile : NetworkBehaviour, IPointerClickHandler
{
    Vector2Int coordinates;

    public void Initialize(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Hex!");
    }
}
