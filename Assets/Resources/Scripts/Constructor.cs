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

	public bool isBuilding = false;
	public bool isFinished = false;

	private Vector3 targetPos;
	public Block targetBlock = null;
	public BlueprintBlock targetBlue = null;


	void Awake() {
		ps = GetComponent<ParticleSystem>();
		text = GameObject.Find("ConstructorText").GetComponent<Text>();
		noise = new Perlin();
	}

	public void Build(Vector3 targetPos) {
		this.targetPos = targetPos;

		if (isBuilding) return;

		oneOverZigs = 1f / (float)zigs;
		ps.enableEmission = false;
		ps.Emit(zigs);
		particles = new ParticleSystem.Particle[ps.maxParticles];
		AlignParticles();

		StartCoroutine("UpdateBuild");

		isBuilding = true;
	}

	public void Build(Block targetBlock) {
		Build(targetBlock.ship.BlockToWorldPos(targetBlock.pos));
	}

	public void StopBuilding() {
		if (!isBuilding) return;

		StopCoroutine("UpdateBuild");
		ps.Clear();

		isBuilding = false;
	}

	IEnumerator UpdateBuild() {
		while (true) {
			isFinished = false;
			if (targetBlue == null && targetBlock == null) {
				isFinished = true;
				yield return new WaitForSeconds(0.1f); // nothing to be done here!
				continue;
			}

			// check if there's a current block we need to get rid of
			bool isRemoving = false;
			if (targetBlock != null) {
				if (targetBlue == null || targetBlue.type != targetBlock.type || targetBlue.orientation != targetBlock.orientation)
					isRemoving = true;
			}

			if (isRemoving) {
				targetBlock.scrapContent -= 1;
				
				if (targetBlock.scrapContent <= 0) {
					targetBlock.ship.blocks[targetBlock.pos] = null;
				}
			} else {
				if (targetBlock == null) { // gotta make a new block
					targetBlock = new Block(targetBlue);
					targetBlue.ship.blocks[targetBlue.pos] = targetBlock;
				}

				if (targetBlock.scrapContent < targetBlock.type.scrapRequired) {
					targetBlock.scrapContent += 1;
				} else {
					isFinished = true;
				}
			}

			yield return new WaitForSeconds(0.01f);
		}
	}
	
	void Update() {
		AlignParticles();

		var ship = Ship.AtWorldPos(targetPos);
		if (ship == null) return;
		var blockPos = ship.WorldToBlockPos(targetPos);
		targetBlue = (BlueprintBlock)ship.blueprint.blocks[blockPos];
		targetBlock = ship.blocks[blockPos];

		//text.text = String.Format("{0}/{1}", targetBlock.scrapContent, targetBlock.type.scrapRequired);
	}	

	void AlignParticles() {
		if (!isBuilding || targetPos == null) return;

		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		var numLiveParticles = ps.GetParticles(particles);

		for (int i = 0; i < particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position, targetPos, oneOverZigs * (float)i);
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