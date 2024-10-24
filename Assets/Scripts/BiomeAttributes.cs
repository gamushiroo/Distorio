using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MinecraftTutorial/Biome Attribute")]
public class BiomeAttributes : ScriptableObject{

    public int solidGroundHeight;

    public int terrainHeight;
    public float terrainScale;

    public float treeZoneScale = 0.15f;
    public float treeZoneThreshold = 0.6f;

    public float treePlacementScale = 0.015f;
    public float treePlacementThreshold = 0.8f;



}
