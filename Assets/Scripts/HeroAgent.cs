using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class HeroAgent : Agent
{
    private MapController mapController;
    private float minDistanceToTarget;

    public override void Initialize()
    {
        mapController = FindObjectOfType<MapController>();
    }

    public override void OnEpisodeBegin()
    {
        minDistanceToTarget = 1000f;
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

        sensor.AddObservation(mapController.GetAgentPositionV3());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        if(action < 5) {
            switch (action)
            {
                case 0:
                    mapController.AgentShoot();
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
            }
        }
        else
        {
            float rotation = (actions.DiscreteActions[0] - 5) * 22.5f;
            mapController.SetAgentRotation(mapController.GetAgentRotation() * Quaternion.Euler(0, 0, rotation - 45));
        }

        float newDistanceToTarget = CalculateDistanceToTarget();
        if(minDistanceToTarget > newDistanceToTarget)
        {
            minDistanceToTarget = newDistanceToTarget;
            AddReward(0.2f);
        }

        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 0;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector3 direction = mousePos - mapController.GetAgentPositionV3();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float prev = mapController.GetAgentRotation().eulerAngles.z;

            discreteActionsOut[0] = 5 + Mathf.RoundToInt(Mathf.Clamp(Mathf.DeltaAngle(prev, angle) + 45f, 0f, 90f) / 22.5f);
        }
    }

    private void MoveAgent(Vector2Int direction)
    {
        Vector2Int newHeroPosition = mapController.GetAgentPosition() + direction;
        if (!mapController.MoveAgent(newHeroPosition))
        {
            AddReward(-0.05f);
        }
    }

    private float CalculateDistanceToTarget()
    {
        Vector2Int heroPosition = mapController.GetAgentPosition();
        Vector2Int enemyPosition = mapController.GetTargetPosition();
        return Vector2Int.Distance(heroPosition, enemyPosition);
    }
}
