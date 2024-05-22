using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class HeroAgent : Agent
{
    private MapController mapController;
    private float lastDistanceToEnemy;

    private float actionCooldown = 0.1f;
    private float lastActionTime = 0f;

    public override void Initialize()
    {
        mapController = FindObjectOfType<MapController>();
        lastDistanceToEnemy = CalculateDistanceToEnemy();
    }

    public override void OnEpisodeBegin()
    {
        mapController.ResetEnvironment();
        lastDistanceToEnemy = CalculateDistanceToEnemy();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<List<GridCell>> grid = mapController.GetLogicalGrid();

        foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                sensor.AddObservation((int)cell);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
/*        float randomDelay = Random.Range(0f, 0.1f);
        if (Time.time - lastActionTime < actionCooldown + randomDelay)
        {
            return;
        }
        lastActionTime = Time.time;*/

        int rotation = actions.DiscreteActions[0];
        mapController.SetHeroRotation(mapController.GetHeroRotation() * Quaternion.Euler(0, 0, rotation - 45));

        int action = actions.DiscreteActions[1];
        switch (action)
        {
            case 0:
                break;
            case 1:
                MoveHero(Vector2Int.up);
                break;
            case 2:
                MoveHero(Vector2Int.down);
                break;
            case 3:
                MoveHero(Vector2Int.right);
                break;
            case 4:
                MoveHero(Vector2Int.left);
                break;
            case 5:
                mapController.HeroShoot();
                break;
        }

        float newDistanceToEnemy = CalculateDistanceToEnemy();
        float distanceDifference = lastDistanceToEnemy - newDistanceToEnemy;

        float reward = distanceDifference * 0.01f;
        AddReward(reward);

        Debug.Log($"Distance to enemy: {newDistanceToEnemy}, Reward: {reward}, Cumulative Reward: {GetCumulativeReward()}");

        lastDistanceToEnemy = newDistanceToEnemy;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - mapController.GetCharacterPosition();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float prev = mapController.GetHeroRotation().eulerAngles.z;

        discreteActionsOut[0] = Mathf.RoundToInt(Mathf.Clamp(Mathf.DeltaAngle(prev, angle) + 45f, 0f, 90f));

        if (Input.GetKey(KeyCode.UpArrow)) discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) discreteActionsOut[1] = 2;
        else if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[1] = 3;
        else if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[1] = 4;
        else if (Input.GetKey(KeyCode.Space)) discreteActionsOut[1] = 5;
        else discreteActionsOut[1] = 0;
    }

    private void MoveHero(Vector2Int direction)
    {
        Vector2Int newHeroPosition = mapController.GetHeroPosition() + direction;
        if (mapController.MoveHero(newHeroPosition))
        {
            lastDistanceToEnemy = CalculateDistanceToEnemy();
        }
        else
        {
            AddReward(-0.1f);
        }
    }

    private float CalculateDistanceToEnemy()
    {
        Vector2Int heroPosition = mapController.GetHeroPosition();
        Vector2Int enemyPosition = mapController.GetEnemyPosition();
        return Vector2Int.Distance(heroPosition, enemyPosition);
    }
}
