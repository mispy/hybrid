using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour {
	ParticleSystem.Particle[] particles;
	ParticleSystem ps;

	// Use this for initialization
	void Start() {
		ps = GetComponent<ParticleSystem>();
		particles = new ParticleSystem.Particle[ps.maxParticles];
		ps.GetParticles(particles);
	}
	
	// Update is called once per frame
	void Update () {
		int num = ps.GetParticles(particles);
		for (var i = 0; i < num; i++) {
		}
		ps.SetParticles(particles, num);
	}
}
