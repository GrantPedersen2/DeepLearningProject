using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;
using System.Linq;
using System.IO;

public class SnakeAgent : Agent
{
    public Transform Target;
    private Transform snakePosition;
    private SnakeAcademy academy;
    public bool HasDied {get;set;} = false;
    List<GameObject> snakeBody = new List<GameObject>();
    private Transform tail;
    private Renderer rend;
    private int gblDirection = 0;
    private object mutex = new object();
    List<string> results = new List<string>();
    bool hasClicked = true;
    bool notHitBody = true;
    public Text score;
    int scoreTotal = 0;

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
        scoreTotal++;
        score.text = "Score: " + scoreTotal;
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

    /// <summary>
    /// Initialize the agent brain and academy for preperation of training
    /// </summary>
    public override void InitializeAgent()
    {
        academy = FindObjectOfType(typeof(SnakeAcademy)) as SnakeAcademy;
        tail = snakePosition = this.transform;
        rend = GetComponent<Renderer>();
        scoreTotal = 0;
        score.text = "Score: " + scoreTotal;
    }

    /// <summary>
    /// Reset agent if the snake dies or eats block
    /// this will start a new episode
    /// </summary>
    public override void AgentReset()
    {
        if (HasDied)
        {
            //Kill the body and rest snake if died...
            //Reset the position
            snakePosition.position = Vector3.zero;
            HasDied = false;
            scoreTotal = 0;
            score.text = "Score: " + scoreTotal;
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

        if(brain.brainType == BrainType.Player)
            WriteFile();
    }

    /// <summary>
    /// Agent brain collects observations only for vector observation
    /// based on tail to target, head to body, and wall to head
    /// </summary>
    public override void CollectObservations()
    {
        // Calculate relative position from tail to target...
        Vector3 relativePosition = Vector3.Normalize(Target.position - tail.position);

        // Relative position
        AddVectorObs(relativePosition.x); // / 5);
        AddVectorObs(relativePosition.z); // / 5);

        // Get position between head and body if possible...
        int index = Mathf.FloorToInt((snakeBody.Count / 2));
        Vector3 bodyPart = Vector3.zero;//(snakeBody.Count > 0) ? Vector3.Normalize(snakePosition.position - snakeBody[index].transform.position) : Vector3.zero;
        AddVectorObs(bodyPart.x);
        AddVectorObs(bodyPart.z);

        //Check surroundings in front of head, is there a body part near as well...
        RaycastHit hit;
        Vector3 fwd = snakePosition.TransformDirection(Vector3.forward);
        
        Debug.DrawRay(snakePosition.position, fwd, Color.green);
        Vector3 hitObj = Vector3.zero;
        if (Physics.Raycast(snakePosition.position, fwd, out hit, 1.0f))
        {
            if(hit.transform.name != "Target")
                hitObj = Vector3.Normalize(hit.transform.position - snakePosition.position);
        }
        AddVectorObs(hitObj.x);
        AddVectorObs(hitObj.z);
        if (hasClicked && notHitBody)
        {
            string matrix = string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}]", relativePosition.x, relativePosition.z, bodyPart.x, bodyPart.z, hitObj.x, hitObj.z, gblDirection);
            Debug.Log(matrix);
            results.Add(matrix);
            notHitBody = hasClicked = false;
        }
    }

    void WriteFile()
    {
        lock (mutex)
        {
            using (StreamWriter writer = new StreamWriter("results.txt", true))
            {
                foreach (var item in results)
                {
                    writer.WriteLine(item);
                }
            }
            results.Clear();
        }
    }
    private void TakeAction(float [] vecAction)
    {
        //Time penalty.
        AddReward(-0.05f);

        MoveAgent(vecAction);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (brain.brainType == BrainType.Player)
        {
            if (Input.anyKeyDown)
            {
                hasClicked = true;
                TakeAction(vectorAction);
            }
        }
        else
        {
            TakeAction(vectorAction);
        }
    }

    /// <summary>
    /// Move the snake agent into a direction based on actions from player or computer
    /// </summary>
    /// <param name="vectorAction"></param>
    private void MoveAgent(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        Vector3 direction = Vector3.zero;
        gblDirection = action;

        switch (action)
        {
            case 0:
                direction = Vector3.left;
                break;
            case 1:
                direction = Vector3.forward;
                break;
            case 2:
                direction = Vector3.right;
                break;
            case 3:
                direction = Vector3.back;
                break;
        }
        notHitBody = snakeBody.Count > 0 ? snakePosition.position + direction != snakeBody[0].transform.position : true;
        if (notHitBody)
        {
            UpdateSnake();
            snakePosition.position += direction;
            snakePosition.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
