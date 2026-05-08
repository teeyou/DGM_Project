using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ½ºÅ¸ÆÃ NPC

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private List<string> _dialogueList = new List<string>();

    [SerializeField] private List<string> _completeList = new List<string>();
    [SerializeField] private ENPC _npcName;
    public void Interact(GameObject target)
    {
        if (target.tag == "Player")
        {
            GameManager.Instance.IsPlayerInteracting = true;
            FieldUIController.Instance.ToggleMenuButton(false);
            FieldUIController.Instance.ToggleQuestButton(false);

            InputManager.Instance.SwitchToMenuUIMap();

            if (GameManager.Instance.HasDigimon)
                FieldUIController.Instance.ShowDialogue(_completeList, _npcName.ToString());
            else
                FieldUIController.Instance.ShowDialogue(_dialogueList, _npcName.ToString());
        }
    }
}
