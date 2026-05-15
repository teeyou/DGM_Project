using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("적 디지몬 상호작용");
        Debug.Log($"_partyList.Count : {_partyList.Count}");
        for (int i = 0; i < _partyList.Count; i++)
        {
            int id = _partyList[i];
            GameManager.Instance.AddBattleList(id);
            Debug.Log($"GameManage.GetBattleList : {GameManager.Instance.GetBattleList().Count}");
        }

        FieldUIController.Instance.ToggleFieldCanvas(false);
        InputManager.Instance.SwitchToBattleMap();

        string currentSceneName = SceneLoader.Instance.GetCurrentSceneName();
        GameManager.Instance.ReturnSceneName = currentSceneName;
        SceneLoader.Instance.LoadTargetScene(currentSceneName + "Battle", false);


    }
}
