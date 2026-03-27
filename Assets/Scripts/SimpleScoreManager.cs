using System.Collections;
using UnityEngine;
using TMPro;

public class SimpleScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public Rigidbody ballRigidbody;
    public Transform ballResetPoint;
    public float resetDelay = 0.1f;

    private int score;
    private bool isResettingBall;

    void Start()
    {
        RefreshScoreUI();
    }

    public void AddGoal()
    {
        score++;
        RefreshScoreUI();
    }

    public void ResetBallAfterGoal()
    {
        if (!isResettingBall)
        {
            StartCoroutine(ResetBallRoutine());
        }
    }

    private void RefreshScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private IEnumerator ResetBallRoutine()
    {
        isResettingBall = true;
        yield return new WaitForSeconds(resetDelay);

        if (ballRigidbody != null && ballResetPoint != null)
        {
            ballRigidbody.position = ballResetPoint.position;
            ballRigidbody.rotation = ballResetPoint.rotation;
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        isResettingBall = false;
    }
}
