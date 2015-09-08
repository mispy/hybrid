using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Constructor : MonoBehaviour
{
	public int zigs = 100;
	public float speed = 1f;
	public float scale = 1f;
	public float range = 3f;
	public Light startLight;
	public Light endLight;
	 
	Perlin noise;
	float oneOverZigs;

	private ParticleSystem ps;
	private ParticleSystem.Particle[] particles;
	private Text text;
	private Crew crew;

	public bool isBuilding = false;

	private Vector3 targetPos;
	public Block targetBlock = null;
	public BlueprintBlock targetBlue = null;

	[Tooltip("Amount of scrap removed per second")]
	public float removeSpeed;
	[Tooltip("Amount of scrap added per second")]
	public float addSpeed;

	void Awake() {
		ps = GetComponent<ParticleSystem>();
		noise = new Perlin();
	}

	public void Build(Vector3 targetPos) {
		this.targetPos = targetPos;

		CancelInvoke("StopBuilding");
		Invoke("StopBuilding", 0.1f);

		if (isBuilding) return;

		oneOverZigs = 1f / (float)zigs;
		ps.enableEmission = false;
		ps.Emit(zigs);
		particles = new ParticleSystem.Particle[ps.maxParticles];

		isBuilding = true;
	}

	public void Build(Block targetBlock) {
		Build(targetBlock.ship.form.BlockToWorldPos(targetBlock.pos));
	}

	public void StopBuilding() {
		if (!isBuilding) return;

		ps.Clear();

		isBuilding = false;
	}

	void UpdateBuild() {
		if (targetBlue == null && targetBlock == null) {
			return;
		}

		var builder = Game.playerShip;
		
		// check if there's a current block we need to get rid of
		bool isRemoving = false;
		if (targetBlock != null) {
			if (targetBlue == null || targetBlue.type != targetBlock.type || targetBlue.orientation != targetBlock.orientation)
				isRemoving = true;
		}
		
		if (isRemoving) {
			var change = removeSpeed*Time.deltaTime;

			targetBlock.scrapContent -= change;
			builder.scrapAvailable += change;

			if (targetBlock.scrapContent <= 0) {
				targetBlock.ship.blocks[targetBlock.pos, targetBlock.layer] = null;
			} else {
				// force a block update
				targetBlue.ship.blocks[targetBlue.pos, targetBlue.layer] = targetBlock;
			}
		} else {
			if (targetBlock == null) { // gotta make a new block
				targetBlock = new Block(targetBlue);
			}
			
			if (targetBlock.scrapContent < targetBlock.type.scrapRequired) {
				var change = addSpeed*Time.deltaTime;
				if (builder.scrapAvailable >= change) {
					builder.scrapAvailable -= change;
					targetBlock.scrapContent += change;
				}
			}

			targetBlue.ship.blocks[targetBlue.pos, targetBlue.layer] = new Block(targetBlock);
		}
	}


	void Update() {
		if (!isBuilding) return;
		var hitPos = targetPos;
		var dist = targetPos - transform.position;
		foreach (var hit in Physics.RaycastAll(transform.position, dist.normalized, dist.magnitude, LayerMask.GetMask(new string[] { "Wall" }))) {
			if (hit.collider != null) {
				hitPos = hit.collider.gameObject.transform.position;
				break;
			}
			Debug.Log(hit.collider);
		}

		AlignParticles(hitPos);

		var form = Blockform.AtWorldPos(hitPos);
		if (form == null) return;

		var blockPos = form.WorldToBlockPos(hitPos);
		targetBlue = (BlueprintBlock)form.blueprint.blocks.Topmost(blockPos);
		targetBlock = form.blocks.Topmost(blockPos);

		UpdateBuild();
		//text.text = String.Format("{0}/{1}", targetBlock.scrapContent, targetBlock.type.scrapRequired);
	}	

	void AlignParticles(Vector2 hitPos) {
		if (!isBuilding || hitPos == null) return;

		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		var numLiveParticles = ps.GetParticles(particles);

		for (int i = 0; i < particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position, hitPos, oneOverZigs * (float)i);
			Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
			                             noise.Noise(timey + position.x, timey + position.y, timey + position.z),
			                             noise.Noise(timez + position.x, timez + position.y, timez + position.z));
			position += (offset * scale * ((float)i * oneOverZigs));
			
			particles[i].position = position;
			particles[i].color = Color.white;
			particles[i].lifetime = 1f;
		}
			
		ps.SetParticles(particles, numLiveParticles);
		
		if (ps.particleCount >= 2)
		{
			if (startLight)
				startLight.transform.position = particles[0].position;
			if (endLight)
				endLight.transform.position = particles[particles.Length - 1].position;
		}
	}
}