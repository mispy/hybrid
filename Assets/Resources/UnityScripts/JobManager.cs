using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Job {
    public Crew crew;
    public abstract void Update();
}

public class MoveJob : Job {
    public readonly IntVector2 targetPos;

    public MoveJob(IntVector2 targetPos) {
        this.targetPos = targetPos;
    }

    public override void Update() {
        if (crew.mind.blockPath.Count == 0 || crew.mind.blockPath.Last() != targetPos)
            crew.mind.PathToBlockPos(targetPos);
    }
}

public class BuildJob : Job {
    public readonly Block targetBlock;

    public BuildJob(Block block) {
        this.targetBlock = block;
    }

    public override void Update() {
        
    }
}

public class BuildJobManager {
    Ship ship;

    List<BuildJob> jobs = new List<BuildJob>();
    IEnumerable<BuildJob> unassignedJobs {
        get { return jobs.Where((job) => job.crew == null); }
    }

    public BuildJobManager(Ship ship) {
        this.ship = ship;
        ship.blueprintBlocks.OnBlockAdded += OnBlueprintUpdate;
        ship.blueprintBlocks.OnBlockRemoved += OnBlueprintUpdate;
    }

    void OnBlueprintUpdate(Block block) {
        jobs.Add(new BuildJob((BlueprintBlock)block));
    }

    public void AssignJobs() {
        foreach (var job in unassignedJobs) {
            foreach (var crew in ship.crew) {
                if (crew.job == null) {
                    crew.job = job;
                    break;
                }
            }
        }
    }
}

public class JobManager : MonoBehaviour {
    Ship ship;
    BuildJobManager buildJobs;

    void Start() {
        ship = GetComponentInParent<Blockform>().ship;
        buildJobs = new BuildJobManager(ship);
    }

    void Update() {
        buildJobs.AssignJobs();
        foreach (var crew in ship.crew) {
            if (crew.job != null)
                crew.job.Update();
        }
    }
}
