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
        return GameManager.Instance.HasDigimon;
    }
}

public class QuestManager : Singleton<QuestManager>
{
    private List<Quest> _questList = new List<Quest>();
    public int CurrentQuestIndex { get; set; } = 0;
    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _questList.Add(new FirstQuest());
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
