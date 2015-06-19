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
	private Block targetBlock;

	Perlin noise;
	float oneOverZigs;

	private ParticleSystem ps;
	private ParticleSystem.Particle[] particles;
	private Text text;
	private bool isBuilding = false;

	void Awake() {
		ps = GetComponent<ParticleSystem>();
		text = GameObject.Find("ConstructorText").GetComponent<Text>();
		noise = new Perlin();
	}
	
	public void StartBuilding(Block targetBlock) {
		if (isBuilding) {
			if (targetBlock != this.targetBlock)
				StopBuilding();
			else
				return;
		}

		if (targetBlock == null) return;
		this.targetBlock = targetBlock;

		Debug.Log("StartBuilding");

		oneOverZigs = 1f / (float)zigs;
		ps.enableEmission = false;
		ps.Emit(zigs);
		particles = new ParticleSystem.Particle[ps.maxParticles];
		AlignParticles();

		StartCoroutine("BuildBlock");

		isBuilding = true;
	}

	public void StopBuilding() {
		if (!isBuilding) return;

		Debug.Log("StopBuilding");

		ps.Clear();
		StopCoroutine("BuildBlock");

		isBuilding = false;
	}

	IEnumerator BuildBlock() {
		while (true) {
			if (targetBlock != null) {
				if (targetBlock.scrapContent != targetBlock.type.scrapRequired) {
					targetBlock.scrapContent += 1;
				}

				if (targetBlock.scrapContent == targetBlock.type.scrapRequired) {
					targetBlock.ship.blocks[targetBlock.pos] = new Block(targetBlock);
				}
			}

			yield return new WaitForSeconds(0.01f);
		}
	}
	
	void Update() {
		if (targetBlock == null) return;

		text.text = String.Format("{0}/{1}", targetBlock.scrapContent, targetBlock.type.scrapRequired);
	
		if (isBuilding) {
			AlignParticles();
		}
	}	

	void AlignParticles() {
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		var numLiveParticles = ps.GetParticles(particles);
		var target = targetBlock.ship.BlockToWorldPos(targetBlock.pos);

		for (int i = 0; i < particles.Length; i++)
		{
			Vector3 position = Vector3.Lerp(transform.position, target, oneOverZigs * (float)i);
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