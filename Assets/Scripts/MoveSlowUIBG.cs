using UnityEngine;

public class MoveSlowUIBG : MonoBehaviour
{
    public float loopDuration = 60f;

    private void Start()
    {
        LeanTween.scale(transform.GetComponent<RectTransform>(), new Vector3(1.25f, 1.12f, 1f), loopDuration)
            .setEaseInOutBounce();
    }
}