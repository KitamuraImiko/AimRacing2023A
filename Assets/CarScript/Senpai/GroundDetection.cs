using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AIM
{
    public class GroundDetection : MonoBehaviour
    {
        public GameObject smokePrefab;
        public GameObject dustPrefab;

        [System.Serializable]
        public class GroundEntity
        {
            public string name;

            public List<int> terrainTextureIndices = new List<int>();

            public List<string> tags = new List<string>();

            public WheelController.FrictionPreset.FrictionPresetEnum frictionPresetEnum;


            [Header("Skidmarks")]
            public Material skidmarkMaterial;
            public bool slipBasedSkidIntensity = false;

            [Header("Particle Effects")]
            [Range(0, 50)]
            public float smokeIntensity = 30f;
            [Range(0, 50)]
            public float dustIntensity = 0f;

            public Color dustColor = Color.yellow;

            [Header("Sound")]
            public bool slipSensitiveSurfaceSound;
            public SoundComponent surfaceSoundComponent;
            public SoundComponent skidSoundComponent;
        }
        [SerializeField]
        public List<GroundEntity> groundEntities = new List<GroundEntity>();

        private Terrain activeTerrain;
        private TerrainData terrainData;
        private Vector3 terrainPos;
        private float[] mix;
        private float[,,] splatmapData;

        [System.Serializable]
        public class SoundComponent
        {
            [Range(0f, 2f)]
            public float volume = 0.6f;

            [Range(0f, 2f)]
            public float pitch = 1f;

            public AudioClip clip;
            [HideInInspector]
            public AudioSource source;
        }

        public int GetCurrentGroundEntityIndex(WheelController wheelController)
        {
            GroundEntity groundEntity = GetCurrentGroundEntity(wheelController);
            if (groundEntity != null)
            {
                return groundEntities.IndexOf(groundEntity);
            }
            else
            {
                return -1;
            }
        }

        public Material GetCurrentSkidmarkTexture(WheelController wheelController)
        {
            GroundEntity groundEntity = GetCurrentGroundEntity(wheelController);
            if (groundEntity != null)
            {
                return GetCurrentGroundEntity(wheelController).skidmarkMaterial;
            }
            else
            {
                return null;
            }
        }

        public GroundEntity GetCurrentGroundEntity(WheelController wheelController)
        {
            if (wheelController.isGrounded && wheelController.wheelHit != null && wheelController.wheelHit.raycastHit.transform != null)
            {
                WheelController.WheelHit hit = null;
                wheelController.GetGroundHit(out hit);

                // タグでチェック
                foreach (GroundEntity groundEntity in groundEntities)
                {
                    if (groundEntity.tags.Count > 0
                        && groundEntity.tags.Contains(wheelController.wheelHit.raycastHit.transform.tag))
                    {
                        return groundEntity;
                    }
                }

                // テラインのチェック
                activeTerrain = hit.raycastHit.transform?.GetComponent<Terrain>();
                if (activeTerrain)
                {
                    int dominantTerrainIndex = GetDominantTerrainTexture(hit.point, activeTerrain);
                    if (dominantTerrainIndex != -1)
                    {
                        foreach (GroundEntity groundEntity in groundEntities)
                        {
                            if (groundEntity.terrainTextureIndices.Count > 0 && groundEntity.terrainTextureIndices.Contains(dominantTerrainIndex))
                            {
                                return groundEntity;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void GetTerrainTextureComposition(Vector3 worldPos, Terrain terrain, ref float[] cellMix)
        {
            try
            {
                terrainData = terrain.terrainData;
                terrainPos = terrain.transform.position;

                int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
                int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

                splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

                cellMix = new float[splatmapData.GetUpperBound(2) + 1];
                for (int n = 0; n < cellMix.Length; ++n)
                {
                    cellMix[n] = splatmapData[0, 0, n];
                }
            }
            catch
            {
                cellMix = null;
            }
        }

        private int GetDominantTerrainTexture(Vector3 worldPos, Terrain terrain)
        {
            GetTerrainTextureComposition(worldPos, terrain, ref mix);
            if (mix != null)
            {
                float maxMix = 0;
                int maxIndex = 0;

                for (int n = 0; n < mix.Length; ++n)
                {
                    if (mix[n] > maxMix)
                    {
                        maxIndex = n;
                        maxMix = mix[n];
                    }
                }
                return maxIndex;
            }
            else
            {
                return -1;
            }
        }
    }
}