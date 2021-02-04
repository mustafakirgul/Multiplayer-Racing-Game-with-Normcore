using UnityEngine;

public class MoveSlowUIBG : MonoBehaviour
{
    private void Start()
    {
        LeanTween.scale(transform.GetComponent<RectTransform>(), new Vector3(1.25f, 1.12f, 1f), 60f)
            .setEaseInOutBounce();
    }
}