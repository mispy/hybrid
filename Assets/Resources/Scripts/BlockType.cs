using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class BlockType : MonoBehaviour {
	public static string Generator = "generator";
	public static string PowerNode = "powerNode";

	[HideInInspector]
	public string name;

	public Texture2D texture;
	public Sprite uiSprite;

	[Tooltip("A complex block has its gameObject instantiated for every block instance. This is expensive!")]
	public bool isComplexBlock = false;

	[Tooltip("The mass value of each block is added to the mass of a ship rigidBody.")]
	public float mass;

	public int scrapRequired = 30;

	[Tooltip("Amount of power consumed by second.")]
	public float powerConsumeRate = 0f;

	[Header("Description")]
	[TextArea]
	public string descriptionHeader;
	[TextArea]
	public string descriptionBody;
	
	// precalculated uv coordinates for each orientation
	[HideInInspector]
	public Vector2[] upUVs;
	[HideInInspector]
	public Vector2[] downUVs;
	[HideInInspector]
	public Vector2[] leftUVs;
	[HideInInspector]
	public Vector2[] rightUVs;

	public Tile tile;
}
