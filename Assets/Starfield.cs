using UnityEngine;
using System.Collections;

public class Starfield : MonoBehaviour {
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.maxParticles];
        ps.maxParticles = 10000;
        ps.Emit(ps.maxParticles);

        var numLiveParticles = ps.GetParticles(particles);
        
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].position = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), -10);
            particles[i].color = Color.white;
            particles[i].lifetime = 100f;
        }

        ps.SetParticles(particles, numLiveParticles);
        ps.Stop();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
