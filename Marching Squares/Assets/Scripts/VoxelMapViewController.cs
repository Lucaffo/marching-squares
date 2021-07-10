using Procedural.Marching.Squares.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Procedural.Marching.Squares.UI
{
    public class VoxelMapViewController : MonoBehaviour
    {
        [Header("Voxel Map to control")]
        public VoxelMap map;
        public NoiseOffsetMover noiseOffsetMover;

        [Header("Commands And Tweaks - Graphics Components")]
        public Slider voxelResolutionSlider;
        public Slider scrollSpeedSlider;
        public Toggle scrollWithTimeToggle;

        public Text gridResolutionText;
        public Text scrollSpeedText;

        private void Start()
        {
            // Setup the UI values
            voxelResolutionSlider.value = map.voxelResolution;
            scrollSpeedSlider.value = noiseOffsetMover.scrollSpeed;
            scrollWithTimeToggle.isOn = noiseOffsetMover.timeScroll;
        }

        private void Update()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            #region Update Commands And Tweaks - Graphics Components

            // Update the value texts
            gridResolutionText.text = map.voxelResolution.ToString();
            scrollSpeedText.text = noiseOffsetMover.scrollSpeed.ToString();

            // Approximante some slider values
            voxelResolutionSlider.value = (int) voxelResolutionSlider.value;

            // Apply the slider values
            map.voxelResolution = (int) voxelResolutionSlider.value;
            noiseOffsetMover.scrollSpeed = scrollSpeedSlider.value;

            // Apply the toggle values
            noiseOffsetMover.timeScroll = scrollWithTimeToggle.isOn;

            #endregion
        }
    }
}