using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const bool TIME_BASED_DISTRACTION = true;
    public const bool CARD_BASED_DISTRACTION = false;


    public CardController cardPrefab;
    public GameObject grid;
    public Vector2 gridSize;
    public Sprite[] frontFacePool;

    private List<CardController> cards = new List<CardController>();
    private CardController flippedCard;

    int numberOfPairs = 0;
    private int pairsFound = 0;


    private bool coroutineRunning = false;
    public int distractionProbability;
    public int timeBombDuration;

    public DistractionType distractionType = DistractionType.None;

    // Distraction dictionary mapping distraction type to its time-based or card-based nature
    private Dictionary<DistractionType, bool> distractionTypes = new Dictionary<DistractionType, bool>
    {
        { DistractionType.TimedBomb, CARD_BASED_DISTRACTION }, // Time-based distraction
        { DistractionType.MovingCards, TIME_BASED_DISTRACTION }, // Card-based distraction
        { DistractionType.DistortedImages, CARD_BASED_DISTRACTION }, // Card-based distraction
        // Add more distractions and their types
    };
    private bool randomTimeDistractionEnable = false;


    public float minDistractionInterval;
    public float maxDistractionInterval;

    private float nextDistractionTime;

    [System.Flags]
    public enum DistractionType
    {
        None = 0,
        TimedBomb = 1 << 0,
        MovingCards = 1 << 1,
        DistortedImages = 1 << 2,
        // Add more distractions as needed
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsTimeBasedDistraction();
        CreateCardGrid();
    }

    private void Update()
    {
        if (randomTimeDistractionEnable && Time.time >= nextDistractionTime)
        {
            ActivateTimeBasedDistraction();

            // Calculate the next distraction time
            nextDistractionTime = Time.time + Random.Range(minDistractionInterval, maxDistractionInterval);
        }
    }

    private void IsTimeBasedDistraction()
    {
        randomTimeDistractionEnable = distractionTypes.ContainsKey(distractionType) && distractionTypes[distractionType];
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

        Vector2 startPosition = new Vector2(-(gridSize.x - 1) * cardSize.x / 2, (gridSize.y - 1) * cardSize.y / 2);

        for (int row = 0; row < gridSize.x; row++)
        {
            for (int col = 0; col < gridSize.y; col++)
            {
                int cardIndex = row * (int)gridSize.y + col;

                if (cardIndex < frontFaceIndices.Count)
                {
                    CardController card = Instantiate(cardPrefab, grid.transform);
                    card.transform.localPosition = startPosition + new Vector2(col * cardSize.x, -row * cardSize.y);

                    card.SetFrontFaceSprite(frontFacePool[frontFaceIndices[cardIndex]]);
                    card.OnCardClicked += OnCardClicked;
                    cards.Add(card);
                }
                else
                {
                    Debug.LogError("Not enough front face indices for the specified number of pairs!");
                    return;
                }
            }
        }
    }

    private void ActivateCardDistraction()
    {
        // Apply the distraction effect based on the distractionType
        if ((distractionType & DistractionType.TimedBomb) != 0)
        {
            Debug.Log("Timed Bomb");
            flippedCard.isLocked = true;
            StartCoroutine(StartTimer());
            // Apply Distraction1 effect here
        }

        if ((distractionType & DistractionType.DistortedImages) != 0)
        {
            Debug.Log("Distraction3");
            // Apply Distraction3 effect here
        }
    }


    private void ActivateTimeBasedDistraction()
    {
        if ((distractionType & DistractionType.MovingCards) != 0)
        {
            Debug.Log("Distraction2");
            // Apply Distraction2 effect here
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
            if (Random.value <= distractionProbability)
            {
                // Apply the distraction effect here
                // You can access the CardController of the flipped card and enable the distraction effect
                ActivateCardDistraction();
            }
        }
        else
        {
            if (flippedCard.frontFaceRenderer.sprite == card.frontFaceRenderer.sprite)
            {
                // Match found
                flippedCard.SetMatched();
                card.SetMatched();

                flippedCard = null;
                pairsFound++;
                if (pairsFound >= numberOfPairs)
                {
                    // All pairs found, game over
                    Debug.Log("Game Over");
                }
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
        if(!flippedCard.isLocked)
        {
            flippedCard.UnflipCard();
            flippedCard = null;
        }
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

    private IEnumerator StartTimer()
    {
        float timer = 0f;

        while (timer < timeBombDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Timer ends, unflip the card if it hasn't been locked
        if(flippedCard != null && flippedCard.isLocked)
            Debug.Log("Bomb Exploded");
    }
}
