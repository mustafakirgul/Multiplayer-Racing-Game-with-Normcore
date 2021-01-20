using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoResizeLootRectTransforms : MonoBehaviour
{
    // Start is called before the first frame update
    float numberOfChildLootObjs;
    void Start()
    {
        ResizeLootScrollBar();
    }

    public void ResizeLootScrollBar()
    {
        RectTransform rt = GetComponent<RectTransform>();

        numberOfChildLootObjs = this.gameObject.transform.childCount;

        rt.sizeDelta = new Vector2(150 * numberOfChildLootObjs, 100);
    }
}
