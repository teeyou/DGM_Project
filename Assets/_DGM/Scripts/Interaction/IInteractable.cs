using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInteractable
{
    public void Interact(GameObject target);
    public UniTask<bool> TryCapture();
}