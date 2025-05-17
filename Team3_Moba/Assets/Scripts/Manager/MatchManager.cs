using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    None,
    Red,
    Blue,
}

public class MatchManager : MonoSingleton<MatchManager>
{
    private MatchCameraController matchCamera;

    private Transform playerTransform;
    private Champion playerChampion;
    private List<Vector3> spawnItemPositions;
    private bool isSpawned;
    private int maxSpawnItem = 20;
    private int currentSpawnCount = 0;

    private Vector3 spawnRedTeamPosition = new Vector3(19f, 6f, 5f);
    private Vector3 spawnBlueTeamPosition = new Vector3(-135f, 6f, -140f);

    public Action<int, int> OnChangedMatchScore;
    public Action<int, int> OnChangedPlayerStat;
    public Action<DateTime> OnUpdateMatchTimer;

    public Transform PlayerTransform => playerTransform;

    protected override void Awake()
    {
        base.Awake();
        TableManager table = new TableManager();
        table.OnLoadGameAction();
    }
    private void Start()
    {
        matchCamera = FindAnyObjectByType<MatchCameraController>();
        playerChampion = FindAnyObjectByType<Champion>();
        playerTransform = playerChampion.transform;
        //아이템 스폰 위치 임시 지정
        spawnItemPositions = new List<Vector3>();
        spawnItemPositions.Add(new Vector3(-34f, 3f, -70f));
        spawnItemPositions.Add(new Vector3(-59f, 3f, -39f));
        spawnItemPositions.Add(new Vector3(-84f, 3f, -65f));
        spawnItemPositions.Add(new Vector3(-60f, 3f, -94f));

        playerChampion.OnDeadComplete += OnChampionDeadComplete;

        UIMatchHUDData matchHUD = new UIMatchHUDData();
        matchHUD.teamScoreText = "0vs0";
        matchHUD.playerStatText = "0/0";
        matchHUD.timerText = "00:00";
        UIManager.Instance.OpenUI<UIMatchHUD>(matchHUD);
        UIChampionHUDData championHUD = new UIChampionHUDData();
        championHUD.champion = playerChampion;
        UIManager.Instance.OpenUI<UIChampionHUD>(championHUD);
        
    }

    private void Update()
    {
        //InputManager
        if (Input.GetMouseButtonDown(1))
        {
            if (playerChampion.GetHP() == 0)
            {
                return;
            }
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //TODO : 스킬매니저에서 예약된 스킬이 있을 때 발동
                if (SkillManager.Instance.CheckReservationSkill())
                {
                    if(SkillManager.Instance.ExecuteSkill(playerChampion, hit) == true)
                    {
                        return;
                    }
                }

                GameEntity entity = hit.collider.gameObject.GetComponent<GameEntity>();
                if (entity != null)
                {
                    playerChampion.SetAttackTarget(entity);
                }
                else
                {
                    playerChampion.ResetAttackTarget();
                    playerChampion.Move(hit.point);
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (playerChampion.GetHP() == 0)
            {
                return;
            }

            SkillTable skill = playerChampion.GetSkillData(SkillInputType.Q);
            //TODO : 쿨타임 체크하는 로직 필요
            if(skill != null)
            {
                //TODO : 바로 시전인지 타겟 설정인지 
                SkillManager.Instance.SetReservationSkill(skill);
                if(skill.excute_type == SkillExecuteType.Immediately)
                {
                    SkillManager.Instance.ExecuteSkill(playerChampion);
                }
            }
        }

        //camera lock - free
        if (Input.GetKeyDown(KeyCode.Space))
        {
            matchCamera.SetMatchCameraState(!matchCamera.IsLocked);
        }

        if(isSpawned == false)
        {
            if(currentSpawnCount >= maxSpawnItem)
            {
                return;
            }

            isSpawned = true;
            StartCoroutine(CoSpawnItem());
        }
    }

    int count = 0;
    IEnumerator CoSpawnItem()
    {
        yield return new WaitForSeconds(3f);
        Vector3 positionTemp = spawnItemPositions[UnityEngine.Random.Range(0,4)];
        float angle = (2f * Mathf.PI / 17) * currentSpawnCount;
        int radius = 4;
        positionTemp.x += Mathf.Cos(angle) * radius;
        positionTemp.z += Mathf.Sin(angle) * radius;
        GameObject item = PoolManager.Instance.SpawnObject("TestItem", positionTemp);
        ExpItem expItem = item.GetComponent<ExpItem>();
        if(expItem != null)
        {
            expItem.Initialize("TestItem", 3, playerChampion.OnGetExpItem);
        }

        if(item != null)
        {
            currentSpawnCount++;
        }

        isSpawned = false;
        count = (count + 1) % 21;
    }

    public void DecreaseExpItemCount()
    {
        Mathf.Min(0, --currentSpawnCount);
    }

    public void OnChampionDeadComplete()
    {

        if (playerChampion.GetTeam() == Team.Red)
        {
            playerChampion.transform.position = spawnRedTeamPosition;
        }
        else if(playerChampion.GetTeam() == Team.Blue)
        {
            playerChampion.transform.position = spawnBlueTeamPosition;
        }
    }
}
