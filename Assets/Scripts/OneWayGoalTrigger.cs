using UnityEngine;

public class OneWayGoalTrigger : MonoBehaviour
{
    public SimpleScoreManager scoreManager;
    public string ballTag = "Ball";
    public bool resetBallOnScore = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(ballTag))
        {
            return;
        }

        if (scoreManager != null)
        {
            scoreManager.AddGoal();
            if (resetBallOnScore)
            {
                scoreManager.ResetBallAfterGoal();
            }
        }
    }
}
