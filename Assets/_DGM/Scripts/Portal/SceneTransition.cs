using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private ESceneId _current;
    [SerializeField] private ESceneId _key;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        SceneLoader.Instance.LoadScene(_current, _key).Forget();
    }

}
