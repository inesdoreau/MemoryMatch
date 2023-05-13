using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public Sprite backFace;
    public SpriteRenderer frontFaceRenderer;
    public GameObject cardBack;
    public GameObject cardFront;

    private bool isFlipped = false;
    private bool isMatched = false;

    public bool IsFlipped
    {
        get { return isFlipped; }
        set { isFlipped = value; }
    }

    public bool IsMatched
    {
        get { return isMatched; }
        set { isMatched = value; }
    }

    public event Action<CardController> OnCardClicked;

    private void Start()
    {
        FlipCard(false);
    }

    public void FlipCard(bool faceUp)
    {
        cardBack.SetActive(!faceUp);
        cardFront.SetActive(faceUp);
        isFlipped = faceUp;
    }

    public void SetMatched()
    {
        isMatched = true;
    }

    public void UnflipCard()
    {
        FlipCard(false);
    }
    
    public void SetFrontFaceSprite(Sprite sprite)
    {
        frontFaceRenderer.sprite = sprite;
    }


    public void OnMouseDown()
    {
        if (!isFlipped && !isMatched && OnCardClicked != null)
        {
            OnCardClicked.Invoke(this);
        }
    }
}
