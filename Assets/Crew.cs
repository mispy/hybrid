using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IOpinionable {

}

public class OpinionReason {
    public static OpinionReason AttackedMyShip = new OpinionReason("Attacked My Ship");
    public static OpinionReason FactionOpinion = new OpinionReason("Opinion Of Faction");
    public static OpinionReason FactionBonus = new OpinionReason("Faction Bonus");
    public static OpinionReason SameFaction = new OpinionReason("Same Faction");

    public readonly string desc;

    public OpinionReason(string desc) {
        this.desc = desc;
    }
}

public class OpinionChange {
    public readonly OpinionReason reason;
    public readonly int amount;

    public OpinionChange(int amount, OpinionReason reason) {
        this.amount = amount;
        this.reason = reason;
    }
}

public class OpinionOf {
    public readonly IOpinionable thing;
    public readonly List<OpinionChange> history = new List<OpinionChange>();
    public readonly List<OpinionChange> bonuses = new List<OpinionChange>();
    public int amount = 0;

    public void Change(int changeAmount, OpinionReason reason) {
        history.Add(new OpinionChange(changeAmount, reason));
        this.amount += changeAmount;
    }

    public OpinionOf(IOpinionable thing) {
        this.thing = thing;
    }
}

public static class CrewManager {
    public static List<Crew> all = new List<Crew>();
    public static Dictionary<string, Crew> byId = new Dictionary<string, Crew>();
    

    public static void Add(Crew crew) {
        CrewManager.all.Add(crew);
        CrewManager.byId[crew.id] = crew;
    }

    public static Crew Create(string name = null, Ship ship = null, Faction faction = null) {
        var names = new string[] { "Fiora", "Anzie", "Abby", "Fiora", "Eldritch" };

        if (name == null) name = Util.GetRandom(names);
        if (faction == null && ship != null) faction = ship.faction;

        var crew = new Crew(name, faction);

        if (ship != null) crew.ship = ship;
        CrewManager.Add(crew);
        return crew;
    }
}

public class CrewOpinion {
    public readonly Crew crew;
    public readonly Dictionary<IOpinionable, OpinionOf> opinions = new Dictionary<IOpinionable, OpinionOf>();

    public CrewOpinion(Crew crew) {
        this.crew = crew;
    }

    public OpinionOf this[Ship ship] {
        get {
            if (!opinions.ContainsKey(ship)) {
                opinions[ship] = new OpinionOf(ship);
                opinions[ship].Change(crew.faction.opinion[ship].amount, OpinionReason.FactionBonus);
            }

            return opinions[ship];
        }
    }
}

[Serializable]
public class Crew : ISaveBindable {
    private Ship _ship;
    public readonly CrewOpinion opinion;
    public int maxHealth = 100;
    public int health = 100;
    public string name;
    public Color color;
    
    public Crew() {
        this.opinion = new CrewOpinion(this);
    }

    public Crew(string name, Faction faction) : this() {
        this.name = name;
        this.faction = faction;
        color = Color.grey;
    }

    public void Savebind(ISaveBinder save) {
        save.BindValue("name", ref name);
        save.BindRef("faction", ref faction);
        save.BindValue("health", ref health);
    }

    public Ship ship {
        get { return _ship; }
        set {
            if (_ship != null) {
                _ship.crew.Remove(this);
            }
            _ship = value;
            _ship.crew.Add(this);
        }
    }

    public string nameWithTitle { 
        get { return "Captain " + name; }
    }

    public string nameWithTitleAndColor {
        get { return String.Format("<color='#{0}'>{1}</color>", ColorUtility.ToHtmlStringRGB(color), nameWithTitle); }
    }
    public string fancyName {
        get { return nameWithTitleAndColor + " of " + _ship.faction.nameWithColor; }
    }

    public Faction faction;

    public CrewBody body;
    public CrewMind mind;

    private Job _job = null;
    public Job job {
        get { return _job; }
        set {
			if (_job != null)
				_job.crew = null;
            _job = value;
            if (value != null) {
				if (value.crew != null)
					value.crew.job = null;
                value.crew = this;
			}
        }
    }


    public override string ToString() {
        return String.Format("Crew<{0}>", this.name);
    }

    public string id {
        get { return name; }
    }
}