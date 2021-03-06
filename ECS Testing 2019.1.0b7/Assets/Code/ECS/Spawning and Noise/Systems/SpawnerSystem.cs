﻿using ECS.Spawning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Spawning.Systems
{
	public class SpawnerSystem : JobComponentSystem
	{
		EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

		protected override void OnCreateManager()
		{
			entityCommandBufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
		}

		struct SpawnerJob : IJobProcessComponentDataWithEntity<Spawner, Translation>
		{
			[ReadOnly] public EntityCommandBuffer CommandBuffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref Spawner spawner, [ReadOnly] ref Translation translation)
			{
				for (float x = 0; x < spawner.SizeX; x++)
				{
					for (float y = 0; y < spawner.SizeY; y++)
					{
						var instance = CommandBuffer.Instantiate(spawner.Prefab);
						CommandBuffer.SetComponent(instance, new Translation
						{
							Value = new float3(x, 0f, y) + translation.Value
						});
					}
				}
				CommandBuffer.DestroyEntity(entity);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new SpawnerJob
			{
				CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
			}.ScheduleSingle(this, inputDeps);

			entityCommandBufferSystem.AddJobHandleForProducer(job);

			return job;
		}
	}
}