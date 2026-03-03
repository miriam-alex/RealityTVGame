using UnityEngine;

public class PlayerInteractable : MonoBehaviour
{
    public ChatBubble chatBubblePrefab;
    private PlayerIdentity id;

    private void Start()
    {
        id = GetComponent<PlayerIdentity>();
    }

    public void Interact(PlayerIdentity interactorId)
    {
        if (id.spotlightOn)
        {
            ChatBubble.Create(chatBubblePrefab, new Vector3(0f, 1.7f, 0f), transform.transform, "MY POINTS!");
            FindObjectOfType<ScoreManager>().StealPoints(id.playerIndex, interactorId.playerIndex);
        }
    }
    
    public void ShowVicinityMessage()
    {
        if (id.spotlightOn)
        {
            GetComponentInChildren<SpotlightVisual>().FlashRed(0.1f);
        }
    }

}
