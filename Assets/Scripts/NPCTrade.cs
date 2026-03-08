using UnityEngine;

public class NPCTrade : MonoBehaviour
{
    private StudentController student;

    public int ballotsRewardMin = 2;
    public int ballotsRewardMax = 5;

    void Start()
    {
        student = GetComponent<StudentController>();
    }

    public void AttemptTrade(PlayerVotes playerVotes)
    {
        if (student == null)
            return;

        if (student.currentPack == null)
            return;

        // Use the student's groupType instead of the pack's groupType
        GroupType1 group = student.groupType;

        int reward = UnityEngine.Random.Range(ballotsRewardMin, ballotsRewardMax + 1);

        playerVotes.AddVotes(reward);

        Debug.Log("Trade completed with " + group + " NPC. Player received " + reward + " ballots.");
    }
}