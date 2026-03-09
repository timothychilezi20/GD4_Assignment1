using UnityEngine;

public class TeacherNPC : MonoBehaviour
{
    [Header("Teacher Penalties")]
    [SerializeField] private float dashRepPenalty = -0.15f;

    public float DashRepPenalty => dashRepPenalty;
}