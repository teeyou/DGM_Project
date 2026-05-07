using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private ESceneId _current;
    [SerializeField] private ESceneId _key;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        if (!GameManager.Instance.HasDigimon)
        {
            FieldUIController.Instance.ShowMessage("보유한 디지몬이 없습니다.\n퀘스트를 완료하세요.");
            return;
        }

        SceneLoader.Instance.LoadScene(_current, _key).Forget();
    }

}
