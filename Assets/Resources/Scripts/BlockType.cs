using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class BlockType : MonoBehaviour {
	[HideInInspector]
	public string name;

	[Tooltip("The name of the block as seen by players.")]
	public string friendlyName;

	public Texture2D texture;
	public Sprite uiSprite;

	[Tooltip("A complex block has its gameObject instantiated for every block instance. This is expensive!")]
	public bool isComplexBlock = false;

	[Tooltip("The mass value of each block is added to the mass of a ship rigidBody.")]
	public float mass;

	public int scrapRequired = 30;

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
}
