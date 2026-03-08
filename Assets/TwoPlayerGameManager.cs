//using UnityEngine;
//using System.Collections.Generic;

//public class TwoPlayerGameManager : MonoBehaviour
//{
//    public static TwoPlayerGameManager Instance { get; private set; }

//    [Header("Player Scores")]
//    [SerializeField] private int player1Votes = 0;
//    [SerializeField] private int player2Votes = 0;

//    [Header("Player Reputations")]
//    [SerializeField] private float player1TeacherRep = 1f;
//    [SerializeField] private float player2TeacherRep = 1f;
//    [SerializeField] private float player1AthleteRep = 1f;
//    [SerializeField] private float player2AthleteRep = 1f;
//    [SerializeField] private float player1ArtistRep = 1f;
//    [SerializeField] private float player2ArtistRep = 1f;
//    [SerializeField] private float player1NerdRep = 1f;
//    [SerializeField] private float player2NerdRep = 1f;

//    [Header("Win Condition")]
//    [SerializeField] private int votesToWin = 50;
//    [SerializeField] private float roundTimeLimit = 180f;

//    private float currentRoundTime = 0f;
//    private int currentRound = 1;

//    public System.Action<int, int> OnPlayerVotesChanged; // playerNumber, votes
//    public System.Action<int> OnRoundChanged;
//    public System.Action<int> OnGameWon; // winning player number

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void Update()
//    {
//        currentRoundTime += Time.deltaTime;

//        // Round timer logic
//        if (currentRoundTime >= roundTimeLimit)
//        {
//            AdvanceToNextRound();
//        }

//        // Check win condition
//        if (player1Votes >= votesToWin)
//        {
//            WinGame(1);
//        }
//        else if (player2Votes >= votesToWin)
//        {
//            WinGame(2);
//        }
//    }

//    public void AddPlayerVotes(int playerNumber, int amount)
//    {
//        if (playerNumber == 1)
//        {
//            player1Votes += amount;
//            OnPlayerVotesChanged?.Invoke(1, player1Votes);
//        }
//        else if (playerNumber == 2)
//        {
//            player2Votes += amount;
//            OnPlayerVotesChanged?.Invoke(2, player2Votes);
//        }
//    }

//    public void ModifyReputation(int playerNumber, NPCMovement.NPCGroup group, float delta)
//    {
//        switch (group)
//        {
//            case NPCMovement.NPCGroup.Nerd:
//                if (playerNumber == 1) player1NerdRep = Mathf.Clamp(player1NerdRep + delta, 0.5f, 1.5f);
//                else player2NerdRep = Mathf.Clamp(player2NerdRep + delta, 0.5f, 1.5f);
//                break;
//            case NPCMovement.NPCGroup.Athlete:
//                if (playerNumber == 1) player1AthleteRep = Mathf.Clamp(player1AthleteRep + delta, 0.5f, 1.5f);
//                else player2AthleteRep = Mathf.Clamp(player2AthleteRep + delta, 0.5f, 1.5f);
//                break;
//            case NPCMovement.NPCGroup.Artist:
//                if (playerNumber == 1) player1ArtistRep = Mathf.Clamp(player1ArtistRep + delta, 0.5f, 1.5f);
//                else player2ArtistRep = Mathf.Clamp(player2ArtistRep + delta, 0.5f, 1.5f);
//                break;
//            case NPCMovement.NPCGroup.Teacher:
//                if (playerNumber == 1) player1TeacherRep = Mathf.Clamp(player1TeacherRep + delta, 0.3f, 1.2f);
//                else player2TeacherRep = Mathf.Clamp(player2TeacherRep + delta, 0.3f, 1.2f);
//                break;
//        }
//    }

//    public float GetPlayerReputation(int playerNumber, NPCMovement.NPCGroup group)
//    {
//        return (playerNumber, group) switch
//        {
//            (1, NPCMovement.NPCGroup.Nerd) => player1NerdRep,
//            (1, NPCMovement.NPCGroup.Athlete) => player1AthleteRep,
//            (1, NPCMovement.NPCGroup.Artist) => player1ArtistRep,
//            (1, NPCMovement.NPCGroup.Teacher) => player1TeacherRep,
//            (2, NPCMovement.NPCGroup.Nerd) => player2NerdRep,
//            (2, NPCMovement.NPCGroup.Athlete) => player2AthleteRep,
//            (2, NPCMovement.NPCGroup.Artist) => player2ArtistRep,
//            (2, NPCMovement.NPCGroup.Teacher) => player2TeacherRep,
//            _ => 1f
//        };
//    }

//    void AdvanceToNextRound()
//    {
//        currentRound++;
//        currentRoundTime = 0f;

//        RoundManager rm = FindFirstObjectByType<RoundManager>();
//        if (rm != null)
//        {
//            rm.currentRound = currentRound;
//        }

//        OnRoundChanged?.Invoke(currentRound);

//        Debug.Log($"Advancing to Round {currentRound}");
//    }

//    void WinGame(int winningPlayer)
//    {
//        Debug.Log($"Player {winningPlayer} Wins!");
//        OnGameWon?.Invoke(winningPlayer);

//        // Stop gameplay
//        Time.timeScale = 0f;
//    }

//    public int GetPlayerScore(int playerNumber) => playerNumber == 1 ? player1Votes : player2Votes;
//    public int GetCurrentRound() => currentRound;
//    public float GetRemainingTime() => Mathf.Max(0, roundTimeLimit - currentRoundTime);
//}