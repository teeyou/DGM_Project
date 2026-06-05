using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyFieldMove : MonoBehaviour, IInteractable
{
    private const float MAX_DISTANCE = 3f;
    
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _moveTimer;
    [SerializeField] private List<int> _partyList;

    private Animator _animator;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private Vector3 _dir;

    private float _timer;
    private bool _isMoving = false;
    public IReadOnlyList<int> PartyList => _partyList;

    private void Awake()
    {
        _startPos = transform.position;
        _animator = GetComponent<Animator>();

        _timer = _moveTimer;
    }

    void Update()
    {
        if (SceneLoader.Instance.IsLoading)
            return;

        if (_moveSpeed == 0)
        {
            return;
        }

        if (!_isMoving)
        {
            _timer -= Time.deltaTime;
            _animator.SetBool("Move", false);
        }

        if (!_isMoving && _timer <= 0f)
        {
            _isMoving = true;
            _timer = _moveTimer;

            _targetPos = GetTargetPos();
            
            _dir = _targetPos - transform.position;
            _dir.y = 0f;
            _dir.Normalize();
            
            transform.rotation = Quaternion.LookRotation(_dir);
        }

        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _moveSpeed * Time.deltaTime);

            //transform.position += _dir * _moveSpeed * Time.deltaTime;

            _animator.SetBool("Move", true);
        }

        if (IsArrived())
        {
            _isMoving = false;
        }
    }

    private Vector3 GetTargetPos()
    {
        float x = Random.Range(-MAX_DISTANCE, MAX_DISTANCE);
        float z = Random.Range(-MAX_DISTANCE, MAX_DISTANCE);

        return new Vector3(_startPos.x + x, 0f, _startPos.z + z);
    }

    private bool IsArrived()
    {
        float dist = Vector3.Distance(transform.position, _targetPos);
        return dist <= 0.01f;
    }

    public void Interact(GameObject target)
    {
        if (this == null)
            return;

        if (target == null)
            return;

        if (_partyList.Count == 0)
        {
            Debug.Log("디지몬 없음");
            return;
        }

        if (_partyList[0] == 7011 && QuestManager.Instance.CurrentQuestIndex < 3)
        {
            FieldUIController.Instance.ShowMessage("이전 퀘스트를 먼저 완료하세요.");
            return;
        }

        else if (_partyList[0] == 7017 && QuestManager.Instance.CurrentQuestIndex < 4)
        {
            FieldUIController.Instance.ShowMessage("이전 퀘스트를 먼저 완료하세요.");
            return;
        }

        else if (_partyList[0] == 6019 && QuestManager.Instance.CurrentQuestIndex < 5)
        {
            FieldUIController.Instance.ShowMessage("이전 퀘스트를 먼저 완료하세요.");
            return;
        }

        if (_partyList.Count == 1)
        {
            int rand = Random.Range(1, 4);
            Debug.Log($"적 디지몬 추가 : {rand}");
            for (int i = 0; i < rand; i++)
            {
                int id = _partyList[0];
                GameManager.Instance.AddBattleList(id);
            }
        }

        else
        {
            for (int i = 0; i < _partyList.Count; i++)
            {
                int id = _partyList[i];
                GameManager.Instance.AddBattleList(id);
            }
        }

        // 몬스터 여러개 생성되는 버그 방지
        // 최대 3마리만 보유 가능함
        var list = GameManager.Instance.GetMutableDigimonStatusList();
        while (list.Count > 3)
        {
            list.RemoveAt(list.Count - 1);
        }

        FieldUIController.Instance.ToggleFieldCanvas(false);
        InputManager.Instance.SwitchToBattleMap();

        string currentSceneName = SceneLoader.Instance.GetCurrentSceneName();
        GameManager.Instance.ReturnSceneName = currentSceneName;
        SceneLoader.Instance.LoadTargetScene(currentSceneName + "Battle", false);

    }

    public async UniTask<bool> TryCapture()
    {
        if (this == null)
            return false;

        if (gameObject == null)
            return false;

        DigimonStatus status = gameObject.GetComponent<DigimonStatus>();
        
        if (status == null)
        {
            Debug.Log("TryCapture - status NULL");
            return false;
        }

        if (GameManager.Instance.GetDigimonStatusList().Count >= 3)
        {
            FieldUIController.Instance.ShowMessage("보유 가능한 디지몬은 최대 3마리 입니다.");
            return false;
        }

        if (status.Grade == EGrade.Baby)
        {
            if (CaptureSystem.Instance.IsCapturePossible(status.DigimonName))
            {
                // 포획 시도
                // 포획 이펙트 키고
                FieldUIController.Instance.ShowMessage("포획 시도...");

                await UniTask.Delay(2500);
                //StartCoroutine(CoDelay(2.5f));

                // 일정 확률로 포획 가능
                float rand = Random.Range(0, 1f);
                Debug.Log($"포획 시도 rand : {rand} - 0.5이상인 경우 포획 성공");
                if (rand < 0.5f)//0.5f
                {
                    FieldUIController.Instance.ShowMessage("포획 실패 (성공 확률 50%)");
                    return false;
                }

                // 포획 성공
                DigimonSpawner.Instance.SpawnCapturedDigimon(status.DigimonName).Forget();
                FieldUIController.Instance.ShowMessage("포획 성공 (성공 확률 50%)");
                return true;
            }
            else
            {
                FieldUIController.Instance.ShowMessage($"퇴치 카운트 부족 : {CaptureSystem.Instance.GetCatchCount(status.DigimonName)} / {CaptureSystem.Instance.RequiredCount}");
                //Debug.Log("포획 불가능 : 캐치카운트 못 채움");
                return false;
            }
        }

        else
        {
            Debug.Log("포획 불가능 : 유년기가 아님");
            return false;
        }
    }
}
