using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Pack
{
    public List<StudentController> students = new List<StudentController>();
    public List<PlayerController> playerControllers = new List<PlayerController>();

    public HangoutZone currentHangout;

    public PackBehavior currentBehavior = PackBehavior.Mingling;

    public float behaviorTimer = 0f;
    public float behaviorDuration = 10f;

    public HangoutZone targetHangout;

    public enum PackBehavior
    {
        //Wandering,
        trading, 
        Mingling,
        MovingToHangout
    }

    public void UpdatePack(float deltaTime)
    {
        behaviorTimer += deltaTime;

        if (behaviorTimer >= behaviorDuration)
        {
            ChooseNextBehavior();
            behaviorTimer = 0f;
        }

        switch (currentBehavior)
        {
            //case PackBehavior.Wandering:
               // HandleWandering();
                //break;

            case PackBehavior.Mingling:
                HandleMingling();
                break;

            case PackBehavior.MovingToHangout:
                HandleMoveToHangout();
                break;
        }
    }



    void HandleWandering()
    {
     
    }

    

    void HandleMingling()
    {
        if (currentHangout == null)
            return;

        foreach (var student in students)
        {
            Vector3 circlePos = currentHangout.transform.position +
                Random.insideUnitSphere * 2f;

            circlePos.y = 0;

            student.MoveToHangout(currentHangout);
        }
    }

    void HandleMoveToHangout()
    {
        if (targetHangout == null)
            return;

        foreach (var student in students)
        {
            student.MoveToHangout(targetHangout);
        }

        currentHangout = targetHangout;
    }

    void ChooseNextBehavior()
    {
        int choice = Random.Range(0, 3);

        currentBehavior = (PackBehavior)choice;
    }
}