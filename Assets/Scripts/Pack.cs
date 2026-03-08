
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class Pack
{
    public List<StudentController> students = new List<StudentController>();
    public HangoutZone currentHangout;
    public PackBehavior currentBehavior;
    public float behaviorTimer = 0f;
    public float behaviorDuration = 10f;

    public enum PackBehavior
    {
        Wandering,      // Move around current hangout
        Mingling,       // Stay in formation, rotate slowly
        MovingToHangout // Travel to another hangout
    }

    public HangoutZone targetHangout; // For MovingToHangout behavior
}

