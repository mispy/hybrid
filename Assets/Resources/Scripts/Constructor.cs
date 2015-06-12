using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Constructor : MonoBehaviour
{
	public Vector3 target;
	public int zigs = 100;
	public float speed = 1f;
	public float scale = 1f;
	public Light startLight;
	public Light endLight;
	
	Perlin noise;
	float oneOverZigs;

	private ParticleSystem ps;
	private ParticleSystem.Particle[] particles;
	private Text text;
	private bool isBuilding = false;

	void Awake() {
		ps = GetComponent<ParticleSystem>();
		text = GameObject.Find("ConstructorText").GetComponent<Text>();
	}
	
	public void StartBuilding() {
		if (isBuilding) return;

		Debug.Log("StartBuilding");

		oneOverZigs = 1f / (float)zigs;
		ps.enableEmission = false;
		ps.Emit(zigs);
		particles = new ParticleSystem.Particle[ps.maxParticles];
		AlignParticles();

		isBuilding = true;
	}

	public void StopBuilding() {
		if (!isBuilding) return;

		Debug.Log("StopBuilding");

		ps.Clear();
		isBuilding = false;
	}
	
	void Update() {
		target = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		var block = Block.BlueprintAtWorldPos(target);
		
		if (block != null) {
			text.text = String.Format("{0}/{1}", block.scrapContent, block.type.scrapRequired);
		}

		if (isBuilding) {
			AlignParticles();
		}
	}	

	void AlignParticles() {
		if (noise == null)
			noise = new Perlin();
		
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		var numLiveParticles = ps.GetParticles(particles);
		
		for (int i=0; i < particles.Length; i++)
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