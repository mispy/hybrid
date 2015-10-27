using UnityEngine;
using System.Collections;

public class NotableEvent {
}

public class NotableEvent_ShipAttacked {
    public Ship attackedShip;
    public Faction attackedFaction;

    public Ship responsibleShip;
    public Faction responsibleFaction;

    public NotableEvent_ShipAttacked(Ship attackedShip, Ship responsibleShip) {
        this.attackedShip = attackedShip;
        this.attackedFaction = attackedShip.faction;
        this.responsibleShip = responsibleShip;
        this.responsibleFaction = responsibleShip.faction;

        responsibleShip.localDisposition[attackedShip] = Disposition.hostile;

        /*foreach (var ship in attackedShip.sector.ships) {
            if (ship.DispositionTowards(attackedShip) == Disposition.friendly) {
                ship.localDisposition[responsibleShip] = Disposition.hostile;
            }
        }*/

        foreach (var crew in attackedShip.crew) {
            crew.opinion[responsibleShip].Change(-10, OpinionReason.AttackedMyShip);
        }
    }
}