using UnityEngine;
using System;
using System.Collections;

public class BeamCannon : BlockType {
    public ParticleSystem ps;
    public Ship ship;
    public Block block;
    
    void Start() {
        ship = transform.parent.gameObject.GetComponent<Ship>();
        ps = GetComponent<ParticleSystem>();
    }
    
    public void Fire() {        
        ps.Emit(1);        
        /*var hitBlocks = Block.FromHits(Util.ParticleCast(beam));
        foreach (var hitBlock in hitBlocks) {
            var ship = hitBlock.ship;
            if (ship == this) continue;
            var newShip = ship.BreakBlock(hitBlock);

            var awayDir = newShip.transform.position - ship.transform.position;
            awayDir.Normalize();
            // make the block fly away from the ship
            newShip.rigidBody.AddForce(awayDir * Block.types["wall"].mass * 1000);

            //var towardDir = newShip.transform.position - beam.transform.position;
            //towardDir.Normalize();
            //newShip.rigidBody.AddForce(towardDir * Block.mass * 100);
        }*/
    }
}
