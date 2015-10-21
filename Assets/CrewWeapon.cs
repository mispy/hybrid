using UnityEngine;
using System.Collections;

public class CrewWeapon : MonoBehaviour {
    float cooldown = 0;

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
    }
    
    public void Fire(Vector2 targetPos) {
        if (cooldown > 0) return;

        var bulletObj = Pool.For("PulseBullet").Attach<Transform>(Game.activeSector.transients);
        var rigid = bulletObj.GetComponent<Rigidbody>();
        var dir = (targetPos - (Vector2)transform.position).normalized;
        rigid.transform.position = transform.position;
        rigid.velocity = dir*20f;

        var col = bulletObj.GetComponent<SphereCollider>();
        var mcol = GetComponent<BoxCollider>();
        Physics.IgnoreCollision(col, mcol);
        
        cooldown = 1f;
    }
}
