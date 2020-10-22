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
        World world = World.DefaultGameObjectInjectionWorld;
        m_entityManager = world.EntityManager;
        m_entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_convered, GameObjectConversionSettings.FromWorld(world, null));
        //InstantiateEntity(new float3(2f, 0f, 4f));
        InstantiateEntityGrid(x, y, spacing);
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
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
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
