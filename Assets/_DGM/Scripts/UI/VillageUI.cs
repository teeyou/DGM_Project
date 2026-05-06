using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageUI : MonoBehaviour
{
    [SerializeField] private GameObject _menuGo;

    public void ToggleMenu(bool enabled)
    {
        _menuGo.SetActive(enabled);
    }
}
