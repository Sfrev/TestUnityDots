using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class JobsExample : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform sprite;

    private List<Yoshi> yoshiList;

    public class Yoshi
    {
        public Transform transform;
        public float moveY;
    };

    private void Start()
    {
        yoshiList = new List<Yoshi>();

        for (int i = 0; i< 1000; i++)
        {
            Transform spriteTransform = Instantiate(sprite, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            yoshiList.Add(new Yoshi { transform = spriteTransform, moveY = UnityEngine.Random.Range(1f, 2f)});
        }
    }

    void Update()
    {
        if (useJobs)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(yoshiList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(yoshiList.Count, Allocator.TempJob);

            for (int i = 0; i < yoshiList.Count; i++)
            {
                positionArray[i] = yoshiList[i].transform.position;
                moveYArray[i] = yoshiList[i].moveY;
            }

            MoveParallelJob moveParallelJob = new MoveParallelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray
            };

            JobHandle moveParallelJobHandle = moveParallelJob.Schedule(yoshiList.Count, 100);
            moveParallelJobHandle.Complete();

            for (int i = 0; i < yoshiList.Count; i++)
            {
                yoshiList[i].transform.position = positionArray[i];
                yoshiList[i].moveY = moveYArray[i];
            }

            positionArray.Dispose();
            moveYArray.Dispose();
        }
        else
        {
            foreach (Yoshi yoshi in yoshiList)
            {
                yoshi.transform.position += new Vector3(0f, yoshi.moveY * Time.deltaTime, 0f);

                if (yoshi.transform.position.y > 5f)
                {
                    yoshi.moveY = -math.abs(yoshi.moveY);
                }
                if (yoshi.transform.position.y < -5f)
                {
                    yoshi.moveY = +math.abs(yoshi.moveY);
                }

                float value = 0f;

                for (int i = 0; i < 1000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }
    }
}

[BurstCompile]
public struct MoveParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index)
    {
        positionArray[index] += new float3(0f, moveYArray[index] * deltaTime, 0f);

        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0f;

        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
