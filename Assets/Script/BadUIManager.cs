using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadUIManager : MonoBehaviour
{    public static BadUIManager Instance { get; private set; }

    public CardController cardPrefab;
    public GameObject grid;
    public Vector2 gridSize;
    public Sprite[] frontFacePool;

    private List<CardController> cards = new List<CardController>();
    private CardController flippedCard;

    int numberOfPairs = 0;
    private int pairsFound = 0;

    private bool coroutineRunning = false;
    public List<Image> images;

    public Sprite emptyNumber;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateCardGrid();
        ResetNumber();
    }

    public void SetNumber(int number, Sprite spriteNumber)
    {
        images[number].sprite = spriteNumber;
    }

    public void ResetNumber()
    {
        foreach (var image in images)
        {
            image.sprite = emptyNumber;
        }
        pairsFound = 0;
    }

    private void CreateCardGrid()
    {
        int totalCards = (int)(gridSize.x * gridSize.y);
        numberOfPairs = totalCards / 2;

        List<int> frontFaceIndices = new List<int>();

        for (int i = 0; i < numberOfPairs; i++)
        {
            frontFaceIndices.Add(i);
            frontFaceIndices.Add(i);
        }

        ShuffleList(frontFaceIndices);

        //Vector2 gridSize = new Vector2(gridColumns, gridRows);
        Vector2 cardSize = cardPrefab.GetComponent<BoxCollider2D>().size;

        Vector2 startPosition = new Vector2((gridSize.x - 1) * cardSize.x / 2, (gridSize.y - 1) * cardSize.y / 2);

        for (int row = 0; row < gridSize.x; row++)
        {
            for (int col = 0; col < gridSize.y; col++)
            {
                int cardIndex = row * (int)gridSize.y + col;

                if (cardIndex < frontFaceIndices.Count)
                {
                    CardController card = Instantiate(cardPrefab, grid.transform);
                    card.transform.localPosition = new Vector2(col * cardSize.x, -row * cardSize.y);

                    card.SetFrontFaceSprite(frontFacePool[frontFaceIndices[cardIndex]]);
                    card.OnCardClicked += OnCardClicked;
                    cards.Add(card);
                }
                else
                {
                    Debug.Log("Not enough front face indices for the specified number of pairs!");
                    return;
                }
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void CardFlipped(CardController card)
    {
        if (flippedCard == null)
        {
            flippedCard = card;
        }
        else
        {
            if (flippedCard.frontFaceRenderer.sprite == card.frontFaceRenderer.sprite)
            {
                // Match found
                //flippedCard.SetMatched();
                //card.SetMatched();
                SetNumber(pairsFound, flippedCard.frontFaceRenderer.sprite);

                pairsFound++;
                StartCoroutine(UnflipCards(card));
            }
            else
            {
                // No match, unflip both cards
                StartCoroutine(UnflipCards(card));
            }

        }
    }

    private IEnumerator UnflipCards(CardController secondCard)
    {
        coroutineRunning = true;

        yield return new WaitForSeconds(1f);
        flippedCard.UnflipCard();
        flippedCard = null;
        secondCard.UnflipCard();

        coroutineRunning = false;
    }

    private void OnCardClicked(CardController card)
    {
        if (coroutineRunning)
            return;

        if (flippedCard == null || (flippedCard != null && flippedCard != card))
        {
            card.FlipCard(true);
            CardFlipped(card);
        }
    }
}
