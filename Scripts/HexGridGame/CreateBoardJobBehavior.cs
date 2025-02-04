using Reflex.Attributes;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityLabs.Slices.Games.Chess;

public class CreateBoardJobBehavior : MonoBehaviour
{
    float m_InitializedTime = 0f;
    public float m_TransitionInDuration = 1f;
    bool m_IsShuttingDown = false;

    [Inject]
    IGameBoard m_Board;

    public float progress = 0f;
    NativeArray<Vector3> targetPositions;
    NativeArray<Pose> InitialPositions;
    JobHandle m_JobHandle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Awake()
    {
        targetPositions = new NativeArray<Vector3>(m_Board.boardPositions.positionList.Count, Allocator.Persistent);
        InitialPositions = new NativeArray<Pose>(m_Board.boardPositions.positionList.Count, Allocator.Persistent);
        for (var i = 0; i < m_Board.boardPositions.positionList.Count; i++)
        {
            targetPositions[i] = m_Board.boardPositions.positionList[i].originalPos;
            var tile = m_Board.boardPositions.positionList[i].transform;
            var pose = InitialPositions[i];
            pose.position = tile.localPosition;
            pose.rotation = tile.localRotation;
            InitialPositions[i] = pose;
        }
        ScheduleJob();
    }

    private void OnEnable()
    {
        m_IsShuttingDown = false;
        m_InitializedTime = Time.time;
    }

    protected void ScheduleJob()
    {
        var job = new CreateBoardJob
        {
            targetPositions = targetPositions,
            InitialPoses = InitialPositions,
            progress = this.progress
        };
        
        m_JobHandle = job.Schedule(m_Board.boardPositions.transformAccessArray);
    }

    protected void UpdateVFX()
    {
        progress = (Time.time - m_InitializedTime) / m_TransitionInDuration;
        if (progress < 1f)
        {
            return;
        }
        progress = 1f;
        ShutDown();
    }

    public void ShutDown()
    {
        m_IsShuttingDown = true;
        enabled = false;
        m_JobHandle.Complete();

        for(int i = 0; i < m_Board.boardPositions.positionList.Count; i++)
        {
            var tile = m_Board.boardPositions.positionList[i].transform;
            tile.transform.localPosition = targetPositions[i];
            tile.transform.localRotation = Quaternion.identity;
            tile.transform.localScale = m_Board.tileGameData.parentScale * Vector3.one;
        }
        m_Board.boardPositions.isBoardCreated = true;
    }

    protected virtual void LateUpdate()
    {
        // Call complete before schedule because its actually completing the job
        // from the prior frame, done so it lets the job run one whole cycle
        m_JobHandle.Complete();
        if (!m_IsShuttingDown)
        {
            UpdateVFX();
        }
        ScheduleJob();
    }

    void OnDestroy()
    {
        targetPositions.Dispose();
        InitialPositions.Dispose();
    }
}

public struct CreateBoardJob : IJobParallelForTransform
{
    public NativeArray<Pose> InitialPoses;
    public NativeArray<Vector3> targetPositions;
    public float progress;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = Vector3.Slerp(InitialPoses[index].position, targetPositions[index], progress);
        transform.rotation = Quaternion.Slerp(InitialPoses[index].rotation, Quaternion.identity, progress);
    }
}
