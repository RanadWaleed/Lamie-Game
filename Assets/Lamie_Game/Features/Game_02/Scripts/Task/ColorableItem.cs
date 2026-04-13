using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorableItem : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ArtColorManager.Instance.SelectObject(gameObject);

    }
}