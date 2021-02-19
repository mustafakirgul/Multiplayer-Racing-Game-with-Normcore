using UnityEngine;


public class UIAnimationEnd : MonoBehaviour
{
    public void AnimationEnded()
    {
        transform.parent.parent.GetComponentInParent<LootCollectionFeedback>().PlayingEnded();
    }
}