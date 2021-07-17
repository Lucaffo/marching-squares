using NoiseGenerator;
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
        public Slider mapResolutionSlider;
        public Slider chunkResolutionSlider;
        public Slider scrollSpeedSlider;
        public Toggle scrollWithTimeToggle;
        public Toggle useInterpolationToggle;
        public Toggle useUvMappingToggle;
        public Toggle useComputeShaderToggle;

        public Text mapResolutionText;
        public Text chunkResolutionText;
        public Text scrollSpeedText;

        [Header("Noise Settings - Graphics Components")]
        public Slider isoLevelSlider;
        public Slider frequenceXSlider;
        public Slider frequenceYSlider;

        public Text isoLevelText;
        public Text frequenceXText;
        public Text frequenceYText;

        [Header("Presets & Debug Settings")]
        public Slider pointGridScaleSlider;
        public Toggle showPointGridToggle;

        public Text pointGridScaleText;

        private void Start()
        {
            SetupView();
        }

        private void Update()
        {
            UpdateView();
        }

        private void SetupView()
        {
            #region Setup Map Settings UI values
            mapResolutionSlider.value = map.chunkResolution;
            chunkResolutionSlider.value = map.voxelResolution;
            useInterpolationToggle.isOn = map.useInterpolation;
            scrollSpeedSlider.value = noiseOffsetMover.scrollSpeed;
            scrollWithTimeToggle.isOn = noiseOffsetMover.timeScroll;
            useUvMappingToggle.isOn = map.useUvMapping;
            useComputeShaderToggle.isOn = map.useComputeShader;
            #endregion

            #region Setup Noise Settings UI values

            isoLevelSlider.value = map.noiseGenerator.isoLevel;
            frequenceXSlider.value = map.noiseGenerator.frequence.x;
            frequenceYSlider.value = map.noiseGenerator.frequence.y;

            #endregion

            #region Setup Preset & Debug Settings UI values

            showPointGridToggle.isOn = map.showVoxelPointGrid;
            pointGridScaleSlider.value = map.voxelScale;

            #endregion
        }

        private void UpdateView()
        {
            #region Update Map Settings - Graphics Components

            // Update the value texts
            mapResolutionText.text = map.chunkResolution.ToString();
            chunkResolutionText.text = map.voxelResolution.ToString();
            scrollSpeedText.text = noiseOffsetMover.scrollSpeed.ToString();

            // Apply the slider values
            map.chunkResolution = (int) mapResolutionSlider.value;
            map.voxelResolution = (int) chunkResolutionSlider.value;
            noiseOffsetMover.scrollSpeed = scrollSpeedSlider.value;

            // Apply the toggle values
            map.useInterpolation = useInterpolationToggle.isOn;
            noiseOffsetMover.timeScroll = scrollWithTimeToggle.isOn;
            map.useUvMapping = useUvMappingToggle.isOn;
            map.useComputeShader = useComputeShaderToggle.isOn;

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

            #endregion

            #region Update Noise Settings - Graphics Components

            // Update the value texts
            pointGridScaleText.text = map.voxelScale.ToString();

            // Update the slider values
            map.voxelScale = pointGridScaleSlider.value;

            // Apply the toggle values
            map.showVoxelPointGrid = showPointGridToggle.isOn;

            #endregion
        }

        public void SetPresetToMap(Noise presetNoise)
        {
            map.noiseGenerator.isoLevel = presetNoise.isoLevel;
            map.noiseGenerator.frequence = presetNoise.frequence;
            map.noiseGenerator.offset = presetNoise.offset;

            SetupView();

            // Refresh the map
            map.Refresh();
        }
    }
}