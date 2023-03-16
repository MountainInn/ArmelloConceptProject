using UnityEngine;
using System.Collections.Generic;

class ViewQueue : MonoBehaviour
{
    public Queue<CanvasGroup> viewsToShow = new Queue<CanvasGroup>();

    public void Add(CanvasGroup canvasGroup)
    {
        viewsToShow.Enqueue(canvasGroup);

        StartCoroutine(KeepShowingNext());
    }

    private System.Collections.IEnumerator KeepShowingNext()
    {
        while (viewsToShow.Count > 0)
        {
            CanvasGroup canvasGroup = viewsToShow.Dequeue();

            if (canvasGroup.alpha == 0f)
                canvasGroup.SetVisibleAndInteractable(true);

            yield return CoroutineExtension.WaitWhile(() => canvasGroup.alpha != 0f);
        }
    }
}
