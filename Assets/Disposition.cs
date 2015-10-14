using UnityEngine;
using System.Collections;

public class Disposition {
    public static Disposition hostile = new Disposition("Hostile", SpaceColor.hostile);
    public static Disposition neutral = new Disposition("Neutral", SpaceColor.neutral);
    public static Disposition friendly = new Disposition("Friendly", SpaceColor.friendly);

    public string name;
    public Color color;

    public override string ToString() {
        return name;
    }


    public static Disposition FromOpinion(OpinionOf opinion) {
        if (opinion.amount > 100)
            return Disposition.friendly;
        else if (opinion.amount < -100)
            return Disposition.hostile;
        else
            return Disposition.neutral;
    }

    public Disposition(string name, Color color) {
        this.name = name;
        this.color = color;
    }
}
