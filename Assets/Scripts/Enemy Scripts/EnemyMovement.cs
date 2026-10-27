using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent enemyNavmeshAgent;
    [SerializeField] private float chaseInterval;
    [SerializeField] private float minHeightChange;
    [SerializeField] private float maxHeightChange;
    [Range(0, 1)]
    [SerializeField] private float heightChangeChance;
    [SerializeField] private float minHeightChangeInterval;
    [SerializeField] private float maxHeightChangeInterval;
    [SerializeField] private float lerpDuration;
    [SerializeField] private AnimationCurve heightChangeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Transform _player;

    
    
    private void Awake()
    {
        InvokeRepeating(nameof(FollowPlayer), 0, chaseInterval);
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(RollChanceToChangeHeight());
    }

    private void FollowPlayer()
    {
        enemyNavmeshAgent.SetDestination(_player.position);
    }

    private IEnumerator RollChanceToChangeHeight()
    {
        if (Random.value <= heightChangeChance)
        {
            float elapsedTime = 0;

            float startOffset = enemyNavmeshAgent.baseOffset;
            float targetOffset = Random.Range(minHeightChange, maxHeightChange);

            while (elapsedTime < lerpDuration)
            {
                float progress = elapsedTime / lerpDuration;
                float curveProgress = heightChangeCurve.Evaluate(progress);

                enemyNavmeshAgent.baseOffset = Mathf.Lerp(startOffset, targetOffset, curveProgress);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        yield return new WaitForSeconds(Random.Range(minHeightChangeInterval, maxHeightChangeInterval));
        StartCoroutine(RollChanceToChangeHeight());
    }
}