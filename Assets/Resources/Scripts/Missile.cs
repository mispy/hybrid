using System;
using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{
	public float explosionForce;
	public float explosionRadius;
	public float detonationImpactVelocity = 10;

	private IEnumerator OnCollisionEnter(Collision col)
	{
		if (enabled && col.contacts.Length > 0) {
			// compare relative velocity to collision normal - so we don't explode from a fast but gentle glancing collision
			float velocityAlongCollisionNormal =
				Vector3.Project(col.relativeVelocity, col.contacts[0].normal).magnitude;
			
			//if (velocityAlongCollisionNormal > detonationImpactVelocity)
			//{
				var explosion = Pool.For("Explosion").TakeObject().GetComponent<Explosion>();
				explosion.transform.position = col.contacts[0].point;
				explosion.transform.rotation = Quaternion.LookRotation(col.contacts[0].normal);
				explosion.gameObject.SetActive(true);
				explosion.Explode(explosionRadius, explosionForce);

				Pool.Recycle(gameObject);
			//}
		}
		
		yield return null;
	}
}
