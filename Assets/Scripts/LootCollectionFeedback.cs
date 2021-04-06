using System;
using UnityEngine;
using UnityEngine.UI;

public class LootCollectionFeedback : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private RawImage image;
    [SerializeField] private Animator[] animators;
    private bool isPlaying;
    public Texture2D[] images;

    private void Awake()
    {
        text = GetComponentInChildren<Text>();
        image = GetComponentInChildren<RawImage>();
        animators = GetComponentsInChildren<Animator>();
    }

    public void PlayAnimation(string _text, int _id)
    {
        if (isPlaying) return;
        text.text = _text;

        if (_id == -1)
        {
            image.texture = images[2];

        }
        else if (_id < 0)
        {
            image.texture = images[0];
        }
        else
        {
            image.texture = images[1];
        }

        //image.texture = images[_id < 0 ? (_id == 0 ? 2 : 0) : 1];
        isPlaying = true;
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].ResetTrigger("play");
            animators[i].SetTrigger("play");
        }
    }

    public void PlayingEnded()
    {
        isPlaying = false;
    }
}