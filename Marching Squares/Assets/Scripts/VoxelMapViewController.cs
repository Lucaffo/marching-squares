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

        [Header("Map Settings - Graphics Components")]
        public Slider voxelResolutionSlider;
        public Slider scrollSpeedSlider;
        public Toggle scrollWithTimeToggle;
        public Toggle useInterpolationToggle;
        public Toggle showPointGridToggle;

        public Text gridResolutionText;
        public Text scrollSpeedText;

        [Header("Noise Settings - Graphics Components")]
        public Slider isoLevelSlider;
        public Slider frequenceXSlider;
        public Slider frequenceYSlider;

        public Text isoLevelText;
        public Text frequenceXText;
        public Text frequenceYText;

        private void Start()
        {
            #region Setup Map Settings UI values
            voxelResolutionSlider.value = map.voxelResolution;
            useInterpolationToggle.isOn = map.useInterpolation;
            showPointGridToggle.isOn = map.showVoxelPointGrid;
            scrollSpeedSlider.value = noiseOffsetMover.scrollSpeed;
            scrollWithTimeToggle.isOn = noiseOffsetMover.timeScroll;
            #endregion

            #region Setup Noise Settings UI values

            isoLevelSlider.value = map.noiseGenerator.isoLevel;
            frequenceXSlider.value = map.noiseGenerator.frequence.x;
            frequenceYSlider.value = map.noiseGenerator.frequence.y;

            #endregion
        }

        private void Update()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            #region Update Map Settings - Graphics Components

            // Update the value texts
            gridResolutionText.text = map.voxelResolution.ToString();
            scrollSpeedText.text = noiseOffsetMover.scrollSpeed.ToString();

            // Apply the slider values
            map.voxelResolution = (int) voxelResolutionSlider.value;
            noiseOffsetMover.scrollSpeed = scrollSpeedSlider.value;

            // Apply the toggle values
            map.useInterpolation = useInterpolationToggle.isOn;
            noiseOffsetMover.timeScroll = scrollWithTimeToggle.isOn;
            map.showVoxelPointGrid = showPointGridToggle.isOn;

            #endregion

            #region Update Noise Settings - Graphics Components

            // Update the value texts
            isoLevelText.text = map.noiseGenerator.isoLevel.ToString();
            frequenceXText.text = map.noiseGenerator.frequence.x.ToString();
            frequenceYText.text = map.noiseGenerator.frequence.y.ToString();

            // Apply the slider values
            map.noiseGenerator.isoLevel = isoLevelSlider.value;
            map.noiseGenerator.frequence.x = frequenceXSlider.value;
            map.noiseGenerator.frequence.y = frequenceYSlider.value;

            // Apply the toggle values

            #endregion
        }
    }
}