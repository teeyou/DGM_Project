using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Quest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Quest(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public abstract bool IsSatisfied();
}

public class FirstQuest : Quest
{
    public FirstQuest() : base("친구 디지몬 받기", "중앙 나무 옆 NPC에게\n친구 디지몬을 받으세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return QuestManager.Instance.QuestCheckList[QuestManager.Instance.CurrentQuestIndex];
        //return GameManager.Instance.HasDigimon;
    }
}

public class SecondQuest : Quest
{
    public SecondQuest() : base("전투에서 승리하기", "오른쪽 포탈로 이동해서\n전투하여 승리하세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return QuestManager.Instance.QuestCheckList[QuestManager.Instance.CurrentQuestIndex];
    }
}

public class ThirdQuest : Quest
{
    public ThirdQuest() : base("디지몬 포획하기", "유년기 디지몬을 잡아서\n퇴치카운트를 모으고\n포획(R)버튼을 눌러서 포획하세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return GameManager.Instance.GetDigimonStatusList().Count >= 2;
    }
}

public class FourthQuest : Quest
{
    public FourthQuest() : base("데빌몬과 전투하여 승리하기", "Temple 맵에 있는\n데빌몬과 전투하여 승리하세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return QuestManager.Instance.QuestCheckList[QuestManager.Instance.CurrentQuestIndex];
    }
}

public class FifthQuest : Quest
{
    public FifthQuest() : base("매그너몬과 전투하여 승리하기", "Temple 맵에 있는\n매그너몬과 전투하여 승리하세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return QuestManager.Instance.QuestCheckList[QuestManager.Instance.CurrentQuestIndex];
    }
}

public class SixthQuest : Quest
{
    public SixthQuest() : base("루체몬과 전투하여 승리하기", "Temple 맵에 있는\n루체몬과 전투하여 승리하세요.")
    {
    }
    public override bool IsSatisfied()
    {
        return QuestManager.Instance.QuestCheckList[QuestManager.Instance.CurrentQuestIndex];
    }
}

public class QuestManager : Singleton<QuestManager>
{
    private List<Quest> _questList = new List<Quest>();
    public int CurrentQuestIndex { get; set; } = 0;
    private List<bool> _questCheckList = new List<bool>();
    public List<bool> QuestCheckList => _questCheckList;
    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        _questList.Add(new FirstQuest());
        _questList.Add(new SecondQuest());
        _questList.Add(new ThirdQuest());
        _questList.Add(new FourthQuest());
        _questList.Add(new FifthQuest());
        _questList.Add(new SixthQuest());

        for (int i = 0; i < _questList.Count; i++)
        {
            _questCheckList.Add(false);
        }

    }

    private void Update()
    {
        if (IsCleared())
        {
            CurrentQuestIndex++;
            FieldUIController.Instance.ShowCurrentQuest();
        }
    }

    public Quest GetCurrentQuest()
    {
        if (CurrentQuestIndex < 0 || CurrentQuestIndex >= _questList.Count)
            return null;

        return _questList[CurrentQuestIndex];
    }

    public bool IsCleared()
    {
        if (CurrentQuestIndex < 0 || CurrentQuestIndex >= _questList.Count)
            return false;

        return _questList[CurrentQuestIndex].IsSatisfied();
    }
}
