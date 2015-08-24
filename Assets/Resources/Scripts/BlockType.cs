using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class BlockComponent : PoolBehaviour {
	public Block block;
}

public class BlockType : BlockComponent {
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

	public Tileable tileable;

	public BlockLayer blockLayer;

	public bool canRotate = false;
	public bool canFitInsideWall = false;
	public bool canBeFired = false;
}