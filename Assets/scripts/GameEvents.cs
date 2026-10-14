using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Src.Units;
using Pathfinding;
using Assets.Src;

public class GameEvents : MonoBehaviour
{

    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<Building> OnBuildCreation; //add building to player list
    public event Action<OnlineBuilding> OnOnlineBuildCreation;
    public void BuildCreation(Building building) => OnBuildCreation?.Invoke(building);
    public void BuildCreation(OnlineBuilding building) => OnOnlineBuildCreation?.Invoke(building);

    public event Action<Unit> OnUnitCreation; //add Unit to player list
    public event Action<OnlineUnit> OnOnlineUnitCreation;
    public void UnitCreation(Unit unit) => OnUnitCreation?.Invoke(unit);
    public void UnitCreation(OnlineUnit unit) => OnOnlineUnitCreation?.Invoke(unit);

    public event Action<int> OnPointsEarn; //add score
    public void PointsEarn(int points) => OnPointsEarn?.Invoke(points);

    public event Action<int[]> OnOnlinePointsEarn; //add score
    public void PointsEarn(int[] points) => OnOnlinePointsEarn?.Invoke(points);

    public event Action<int> OnResourceEarn; //add resources
    public void ResourceEarn(int points) => OnResourceEarn?.Invoke(points);

    public event Action<int> OnSpResourceEarn; //add resources
    public void SpResourceEarn(int points) => OnSpResourceEarn?.Invoke(points);

    public event Action<int[]> OnOnlineResourceEarn;
    public void ResourceEarn(int[] points) => OnOnlineResourceEarn?.Invoke(points);

    public event Action<int[]> OnOnlineSpResourceEarn;
    public void SpResourceEarn(int[] points) => OnOnlineSpResourceEarn?.Invoke(points);

    public event Action<Target> OnClickInformation; //give information about clicked object
    public event Action<OnlineTarget> OnOnlineClickInformation;
    public void ClickInformation(Target target) => OnClickInformation?.Invoke(target);
    public void ClickInformation(OnlineTarget target) => OnOnlineClickInformation?.Invoke(target);

    public event Action<string> OnShowMessage;  //shows message
    public void ShowMessage(string message) => OnShowMessage?.Invoke(message);

    public event Action<bool> OnStartBuilding; //on start of a building
    public void StartBuilding(bool value) => OnStartBuilding?.Invoke(value);

    public event Action<Target> OnDeath; //on death of a target
    public void Death(Target target) => OnDeath?.Invoke(target);

    public event Action<OnlineTarget> OnOnlineDeath;
    public void Death(OnlineTarget target) => OnOnlineDeath?.Invoke(target);

    public event Action<int> OnUseAbility; //Use ability
    public void UseAbility(int index) => OnUseAbility?.Invoke(index);

    public event Action OnLevelUp; // on leveling hero
    public void LevelUp() => OnLevelUp?.Invoke();

    public event Action<GameObject> OnTargetAbility;
    public void TargetAbility(GameObject ability) => OnTargetAbility?.Invoke(ability);

    public event Action<int> OnOnlineTargetAbility;
    public void TargetAbility(int id) => OnOnlineTargetAbility?.Invoke(id);

    public event Action OnMineDestroy;
    public void MineDestroy() => OnMineDestroy?.Invoke();

    public event Action<int> OnCapacitySet;
    public void CapacitySet(int value) => OnCapacitySet?.Invoke(value);

    public event Action<int[]> OnOnlineCapacitySet;
    public void CapacitySet(int[] value) => OnOnlineCapacitySet?.Invoke(value);

    public event Action<string> OnNewWave;
    public void NewWave(string text) => OnNewWave?.Invoke(text);

    public event Action OnEnemyCreation; //add Unit to player list
    public void EnemyCreation() => OnEnemyCreation?.Invoke();

    public event Action<bool> OnBlueprintStart;
    public void BlueprintStart(bool start) => OnBlueprintStart?.Invoke(start);

    public event Action<int[]> OnOnlineBlueprintStart;
    public void OnlineBlueprintStart(int[] values) => OnOnlineBlueprintStart?.Invoke(values);

    public event Action<bool> OnBlueprintGeneratorStart;
    public void BlueprintGeneratorStart(bool start) => OnBlueprintGeneratorStart?.Invoke(start);

    public event Action<int[]> OnOnlineBlueprintGeneratorStart;
    public void OnlineBlueprintGeneratorStart(int[] values) => OnOnlineBlueprintGeneratorStart?.Invoke(values);

    public event Action<bool> OnGameFinished;
    public void GameFinished(bool win) => OnGameFinished?.Invoke(win);

    public event Action OnHeroDeath;
    public void HeroDeath() => OnHeroDeath?.Invoke();

    public event Action<int> OnOnlineHeroDeath;
    public void OnlineHeroDeath(int id) => OnOnlineHeroDeath?.Invoke(id);

    public event Action<Vector3> OnHeroRevived;
    public void HeroRevived(Vector3 position) => OnHeroRevived?.Invoke(position);

    public event Action<OnlineBuilding> OnOnlineHeroRevived;
    public void OnlineHeroRevived(OnlineBuilding building) => OnOnlineHeroRevived?.Invoke(building);

    public event Action OnBossPartDeath;
    public void BossPartDeath() => OnBossPartDeath?.Invoke();

    public event Action OnCancelAbility;
    public void CancelAbility() => OnCancelAbility?.Invoke();

    public event Action<Vector2> OnMinimapClick;
    public void MinimapClick(Vector2 vec) => OnMinimapClick?.Invoke(vec);

    public event Action<int> OnCreatingUnit;
    public void CreatingUnit(int cost) => OnCreatingUnit?.Invoke(cost);

    public event Action<GraphNode> OnGridUpdate;
    public void GridUpdate(GraphNode node) => OnGridUpdate?.Invoke(node);

    public event Action<Base> OnBaseCreation;
    public event Action<OnlineBase> OnOnlineBaseCreation;
    public void BaseCreation(Base building) => OnBaseCreation?.Invoke(building);
    public void BaseCreation(OnlineBase building) => OnOnlineBaseCreation?.Invoke(building);

    public event Action<float[]> OnPreBuilding;
    public void PreBuilding(float[] values) => OnPreBuilding?.Invoke(values);

    public event Action<OnlineMines> OnMineCreation;
    public void MineCreation(OnlineMines val) => OnMineCreation?.Invoke(val);

    public event Action<OnlinePlayerManager> OnManagerCreation;
    public void ManagerCreation(OnlinePlayerManager manager) => OnManagerCreation?.Invoke(manager);

    public event Action<string> OnOnlineGetMessage;
    public void GetMessage(string s) => OnOnlineGetMessage?.Invoke(s);

    public event Action OnOnlineUnloadScene;
    public void OnlineUnloadScene() => OnOnlineUnloadScene?.Invoke();

}
