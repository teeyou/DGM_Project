using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ½ºÅ¸ÆÃ NPC

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private List<string> _dialogueList = new List<string>();
    [SerializeField] private ENPC _npcName;
    public void Interact(GameObject target)
    {
        if (target.tag == "Player")
        {
            FieldUIController.Instance.ShowDialogue(_dialogueList, _npcName.ToString());
        }
    }
}
