using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class HeroAgent : Agent
{
    private MapController mapController;
    private float minDistanceToTarget = 1000f;

    public override void Initialize()
    {
        mapController = FindObjectOfType<MapController>();
    }

    public override void OnEpisodeBegin()
    {
        mapController.ResetEnvironment();
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
        int rotation = actions.DiscreteActions[0];
        mapController.SetAgentRotation(mapController.GetAgentRotation() * Quaternion.Euler(0, 0, rotation - 45));

        int action = actions.DiscreteActions[1];
        switch (action)
        {
            case 0:
                break;
            case 1:
                MoveAgent(Vector2Int.up);
                break;
            case 2:
                MoveAgent(Vector2Int.down);
                break;
            case 3:
                MoveAgent(Vector2Int.right);
                break;
            case 4:
                MoveAgent(Vector2Int.left);
                break;
            case 5:
                mapController.AgentShoot();
                break;
        }

        float newDistanceToTarget = CalculateDistanceToTarget();
        if(minDistanceToTarget > newDistanceToTarget)
        {
            minDistanceToTarget = newDistanceToTarget;
            AddReward(0.1f);
        }

        Debug.Log($"Distance to enemy: {newDistanceToTarget}, Cumulative Reward: {GetCumulativeReward()}");

        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - mapController.GetAgentPositionV3();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float prev = mapController.GetAgentRotation().eulerAngles.z;

        discreteActionsOut[0] = Mathf.RoundToInt(Mathf.Clamp(Mathf.DeltaAngle(prev, angle) + 45f, 0f, 90f));

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[1] = 2;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 3;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 4;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[1] = 5;
        }
        else
        {
            discreteActionsOut[1] = 0;
        }
    }

    private void MoveAgent(Vector2Int direction)
    {
        Vector2Int newHeroPosition = mapController.GetAgentPosition() + direction;
        if (!mapController.MoveAgent(newHeroPosition))
        {
            AddReward(-0.03f);
        }
    }

    private float CalculateDistanceToTarget()
    {
        Vector2Int heroPosition = mapController.GetAgentPosition();
        Vector2Int enemyPosition = mapController.GetTargetPosition();
        return Vector2Int.Distance(heroPosition, enemyPosition);
    }
}
