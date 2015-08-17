using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class BlockType : MonoBehaviour {
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

	public Tileable tileable;

	public int blockLayer = Block.baseLayer;
}