using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class SnakeAgent : Agent
{
    public Transform Target;
    private Transform snakePosition;
    private SnakeAcademy academy;
    public bool HasDied {get;set;} = false;
    List<GameObject> snakeBody = new List<GameObject>();
    private Transform tail;
    private Renderer rend; 

    void AddBodyPart()
    {
        GameObject part = Instantiate(Resources.Load<GameObject>("SnakeBody")) as GameObject;

        if (!snakeBody.Contains(part))
        {
            snakeBody.Add(part);
        }

        //Put behind object...
        part.transform.position = tail.localPosition - tail.forward;
        tail = part.transform;
    }

    void DestroyBody()
    {
        foreach(var body in snakeBody)
            Destroy(body);

        snakeBody.Clear();
        tail = snakePosition;
    }

    void UpdateSnake()
    {
        if (snakeBody.Count > 0)
        {
            for (int i = snakeBody.Count - 1; i > 0; i--)
            {
                snakeBody[i].transform.position = snakeBody[i - 1].transform.position;
                snakeBody[i].transform.rotation = snakeBody[i - 1].transform.rotation;
            }
            snakeBody[0].transform.position = snakePosition.position;
            snakeBody[0].transform.rotation = snakePosition.rotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.name == "Target")
        {
            //AddBody part...
            AddBodyPart();
            AddReward(1.0f);
        }
        else
        {
            HasDied = true;
            AddReward(-1.0f);
            DestroyBody();
        }
        Done();
    }

    public override void InitializeAgent()
    {
        academy = FindObjectOfType(typeof(SnakeAcademy)) as SnakeAcademy;
        tail = snakePosition = this.transform;
        rend = GetComponent<Renderer>();
    }

    public override void AgentReset()
    {
        if (HasDied)
        {
            //Kill the body and rest snake if died...
            //Reset the position
            snakePosition.position = Vector3.zero;
            HasDied = false;
        }
        else
        {
            var position = Vector3.zero;
            do
            {
                float x = Mathf.Floor(Random.value * 8 - 4);
                float z = Mathf.Floor(Random.value * 8 - 4);

                position = new Vector3(x, Target.position.y, z);
            } while (snakeBody.Any(x => x.GetComponent<Renderer>().bounds.Contains(position)) || rend.bounds.Contains(position));

            Target.position = position;
        }
        //academy.AcademyReset();
    }

    public override void CollectObservations()
    {
        // Calculate relative position from tail to target...
        Vector3 relativePosition = Vector3.Normalize(Target.position - tail.position);

        // Relative position
        AddVectorObs(relativePosition.x);
        AddVectorObs(relativePosition.z);

        // Get position between head and body if possible...
        Vector3 bodyPart = (snakeBody.Count > 0) ? Vector3.Normalize(snakePosition.position - snakeBody[snakeBody.Count / 2].transform.position) : Vector3.zero;
        AddVectorObs(bodyPart.x);
        AddVectorObs(bodyPart.z);

        // Distance to walls
        /*AddVectorObs((snakePosition.position.x + 5) / 5);
        AddVectorObs((snakePosition.position.x - 5) / 5);
        AddVectorObs((snakePosition.position.z + 5) / 5);
        AddVectorObs((snakePosition.position.z - 5) / 5);*/
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (brain.brainType == BrainType.Player)
        {
            if (Input.anyKeyDown)
            {
                /*if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
                {

                }*/

                //Time penalty.
                AddReward(-0.05f);

                MoveAgent(vectorAction);
            }
        }
        else
        {
            //Time penalty.
            AddReward(-0.05f);

            MoveAgent(vectorAction);
        }
    }

    /// <summary>
    /// Move the snake agent into a direction without rotating the object.
    /// </summary>
    /// <param name="vectorAction"></param>
    private void MoveAgent(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        Vector3 direction = Vector3.zero;
        UpdateSnake();

        switch (action)
        {
            case -1:
                direction = Vector3.left;
                break;
            case 0:
                direction = Vector3.forward;
                break;
            case 1:
                direction = Vector3.right;
                break;
            case 2:
                direction = Vector3.back;
                break;
        }

        snakePosition.position += direction;
        snakePosition.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
