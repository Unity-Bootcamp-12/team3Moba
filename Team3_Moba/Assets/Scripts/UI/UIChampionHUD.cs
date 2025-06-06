using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChampionHUDData : UIBaseData
{
    public Champion champion;
}

public class UIChampionHUD : UIBase
{
    [SerializeField] private Image championImage;
    [SerializeField] private TextMeshProUGUI championLevelText;
    [SerializeField] private Image coverImage;
    [SerializeField] private TextMeshProUGUI championRespawnText;
    [SerializeField] private Image championHPSlider;
    [SerializeField] private TextMeshProUGUI championHPText;
    [SerializeField] private Image championExpSlider;
    [SerializeField] private TextMeshProUGUI championExpText;
    [SerializeField] private Transform skillSlotLayout;

    public override void SetInfo(UIBaseData uidata)
    {
        base.SetInfo(uidata);
        UIChampionHUDData data = uidata as UIChampionHUDData;

        if (data.champion == null)
        {
            Logger.LogError("Player Champion is null");
            return;
        }
        ChampionTable table = TableManager.Instance.FindTableData<ChampionTable>(data.champion.GetEntityID());
        championImage.sprite = Resources.Load<Sprite>($"Textures/Champion/{table.champion_icon}");
        championLevelText.text = data.champion.CurrentLevel.ToString();
        championHPText.text = $"{table.hp} / {table.hp}";
        championHPSlider.fillAmount = table.hp / table.hp;
        LevelTable levelTable = TableManager.Instance.FindTableData<LevelTable>(1);
        championExpText.text = $"{0} / {levelTable.require_exp}";
        championExpSlider.fillAmount = 0;
        Bind(data.champion);
        SetChampionSkillSlot(data.champion);
    }

    private void SetChampionSkillSlot(Champion champion)
    {
        //Skill Slot Spawn with Bind CoolTimeManager
        GameObject slotObj = Resources.Load<GameObject>("UI/Slot/UISkillSlot");
        foreach (SkillInputType skillInput in Enum.GetValues(typeof(SkillInputType)))
        {
            SkillTable skillTable = champion.GetSkillData(skillInput);
            if(skillTable != null)
            {
                UISkillSlotData skillSlotData = new UISkillSlotData();
                skillSlotData.slotType = UISlotType.Skill;
                skillSlotData.slotIcon = Resources.Load<Sprite>($"Textures/Skill/{skillTable.skill_icon}");
                skillSlotData.skillInputType = skillInput;
                skillSlotData.skillTable = skillTable;
                skillSlotData.champion = champion;
                UISkillSlot skillSlot = Instantiate(slotObj, skillSlotLayout).GetComponent<UISkillSlot>();
                skillSlot.SetInfo(skillSlotData);
            }
        }
    }

    //TODO : Event 연결 (챔피언 데이터와 동기화)
    public void Bind(Champion champion)
    {
        champion.OnLevelChanged += OnUpdateLevel; 
        champion.OnHPChanged += OnUpdateHP;
        champion.OnExpChanged += OnUpdateExp;
        champion.OnDead += OnDeadAction;
        champion.OnChampionRespawnTimer += OnUpdateRespawnTimer;
        champion.OnDeadComplete += OnDeadCompleteAction;
    }

    public void OnUpdateLevel(int currentLevel)
    {
        championLevelText.text = currentLevel.ToString();
    }

    public void OnUpdateHP(float currentHP, float maxHP)
    {
        championHPText.text = $"{currentHP.ToString("F0")} / {maxHP.ToString("F0")}";
        championHPSlider.fillAmount = currentHP / maxHP;
    }

    public void OnUpdateExp(float currentExp, float maxExp)
    {
        championExpText.text = $"{currentExp.ToString("F0")} / {maxExp.ToString("F0")}";
        championExpSlider.fillAmount = currentExp / maxExp;
    }

    public void OnDeadAction()
    {
        coverImage.gameObject.SetActive(true);
        championRespawnText.gameObject.SetActive(true);
    }
    public void OnUpdateRespawnTimer(float respawnTime)
    {
        championRespawnText.text = $"{respawnTime.ToString("F0")}";
    }
    public void OnDeadCompleteAction()
    {
        coverImage.gameObject.SetActive(false);
        championRespawnText.gameObject.SetActive(false);
        championRespawnText.text = string.Empty;
    }
}
