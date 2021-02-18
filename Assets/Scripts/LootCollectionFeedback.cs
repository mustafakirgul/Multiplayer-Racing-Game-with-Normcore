using UnityEngine;
using UnityEngine.UI;

public class LootCollectionFeedback : MonoBehaviour
{
    private Text text;
    private Image image;
    private Animator[] animators;

    private void Start()
    {
        text = GetComponentInChildren<Text>();
        image = GetComponentInChildren<Image>();
        animators = GetComponentsInChildren<Animator>();
    }
}
