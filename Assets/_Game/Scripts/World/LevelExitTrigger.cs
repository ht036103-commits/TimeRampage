using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelExitTrigger : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private VictoryUI victoryUI;

    private bool playerAlreadyReached;

    private void Reset()
    {
        BoxCollider boxCollider =
            GetComponent<BoxCollider>();

        boxCollider.isTrigger = true;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (playerAlreadyReached)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (victoryUI == null)
        {
            Debug.LogError(
                "LevelExitTrigger: Chua gan VictoryUI!",
                this
            );

            return;
        }

        playerAlreadyReached = true;

        victoryUI.PlayerReachedExit();
    }

}
