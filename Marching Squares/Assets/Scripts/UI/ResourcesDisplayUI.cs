using System;

using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Procedural.Marching.Squares.UI
{
    public class ResourcesDisplayUI : MonoBehaviour
    {
		[Header("Map")]
		public VoxelMap map;

		[Header("UI Components")]
		public Text fpsCounterText;
		public Text totalVerticesText;
		public Text totalTrianglesText;

		[Header("Resource monitoring settings")]
		public float updateTimeSeconds = 1;

		private float fps;
		private int totalVertices;
		private int totalTriangles;

		private Coroutine monitoringCoroutine;

		private void OnEnable()
		{
			StartResourceMonitoring();
		}

		private void OnDisable()
        {
			StopResourceMonitoring();
		}

		public void StartResourceMonitoring()
        {
			if(monitoringCoroutine == null)
            {
				monitoringCoroutine = StartCoroutine(MonitoringCoroutine());
            }
        }

		public void StopResourceMonitoring()
        {
			if(monitoringCoroutine != null)
            {
				StopCoroutine(monitoringCoroutine);
            }
        }

		private IEnumerator MonitoringCoroutine()
        {
			while(true)
			{
				totalTriangles = 0;
				totalVertices = 0;

				for (int i = 0; i < map.chunks.Count; i++)
				{
					totalVertices += map.chunks[i].mesh.vertexCount;
					totalTriangles += map.chunks[i].mesh.triangles.Length;
				}

				fps = (1F / Time.unscaledDeltaTime);

				fpsCounterText.text = fps.ToString("0.0") + " FPS";
				totalVerticesText.text = totalVertices.ToString() + " vertices";
				totalTrianglesText.text = totalTriangles.ToString() + " triangles";


				yield return new WaitForSeconds(updateTimeSeconds);
            }
        }
	}

}
