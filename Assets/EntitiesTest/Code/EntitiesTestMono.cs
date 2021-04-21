using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using SoftLiu.EntitiesTest;

public class EntitiesTestMono : MonoBehaviour
{
    [SerializeField]
    private GameObject m_convered;

    [SerializeField]
    private int x = 10;
    [SerializeField]
    private int y = 10;
    [SerializeField]
    [Range(0.1f, 2f)]
    private float spacing = 0.1f;
    private Entity m_entityPrefab;
    private EntityManager m_entityManager;

    private void Start()
    {
        BlobAssetStore store = new BlobAssetStore();
        World world = World.DefaultGameObjectInjectionWorld;
        m_entityManager = world.EntityManager;
        m_entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_convered, GameObjectConversionSettings.FromWorld(world, store));
        //InstantiateEntity(new float3(2f, 0f, 4f));
        InstantiateEntityGrid(x, y, spacing);
        store.Dispose();
    }

    private void InstantiateEntity(float3 position)
    {
        Entity myEntity = m_entityManager.Instantiate(m_entityPrefab);
        m_entityManager.SetComponentData(myEntity, new Translation()
        {
            Value = position
        });
    }
    private void InstantiateEntityGrid(int x, int y, float spacing = 1f)
    {
        int startX = x / 2;
        int startY = y / 2;
        for (int i = -startX; i < startX; i++)
        {
            for (int j = -startY; j < startY; j++)
            {
                Entity myEntity = m_entityManager.Instantiate(m_entityPrefab);
                m_entityManager.SetComponentData(myEntity, new Translation()
                {
                    Value = new float3(i + spacing, j + spacing, 0)
                });
            }
        }

    }
}
