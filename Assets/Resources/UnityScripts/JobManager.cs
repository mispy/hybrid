using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Job {
    public Crew crew;
    public bool isFinished;
    public abstract void Update();
}

public class MoveJob : Job {
    public readonly IntVector2 targetPos;

    public MoveJob(IntVector2 targetPos) {
        this.targetPos = targetPos;
    }

    public override void Update() {
        crew.mind.SetMoveDestination(targetPos);
        if (crew.body.currentBlockPos == targetPos)
            isFinished = true;
    }
}

public class BuildJob : Job {
    public readonly BlueprintBlock targetBlue;

    public BuildJob(Block block) {
        this.targetBlue = (BlueprintBlock)block;
    }

    public bool AcceptCrew(Crew crew) {
        var neighbors = IntVector2.NeighborsWithDiagonal(targetBlue.pos).OrderBy((n) => IntVector2.Distance(crew.body.currentBlockPos, n));
        foreach (var neighbor in neighbors) {
            if (crew.mind.CanReach(neighbor)) {
				Debug.Log(targetBlue.ship.blocks[neighbor, targetBlue.layer]);
				crew.mind.SetMoveDestination(neighbor);
                crew.job = this;
				Debug.LogFormat("Build job assigned to {0}", crew);
                return true;
            }
        }

		//Debug.LogFormat("{0} cannot path to build {1}", crew, targetBlue);
        return false;
    }

	public override string ToString() {
		return String.Format("BuildJob<{0}>", targetBlue);
	}

    public override void Update() {
		if (targetBlue.ship == null) {
			isFinished = true;
			return;
		}

        if (targetBlue.ship.blocks[targetBlue.pos, targetBlue.layer] != null && targetBlue.ship.blocks[targetBlue.pos, targetBlue.layer].type == targetBlue.type) {
			Debug.LogFormat("Successfully built: {0}", targetBlue);
            isFinished = true;
            return;
        }

        if (IntVector2.Distance(crew.body.currentBlockPos, targetBlue.pos) < 2) {
            crew.body.constructor.Build(targetBlue);
            return;
        } else if (crew.mind.currentDest == null) {
			if (!AcceptCrew(crew)) {
				crew.job = null;
				return;
			}
		}
		

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
        //ship.blueprintBlocks.OnBlockRemoved += OnBlueprintUpdate;
    }

    void OnBlueprintUpdate(Block block) {
        jobs.Add(new BuildJob(block));
    }

    public void AssignJobs() {
		foreach (var job in jobs.ToList()) {
			if (job.isFinished)
				jobs.Remove(job);
		}

		foreach (var crew in ship.crew) {
			if (crew.job != null) continue;

			var nearestJobs = unassignedJobs.OrderBy((j) => IntVector2.Distance(crew.body.currentBlockPos, j.targetBlue.pos));

			foreach (var job in nearestJobs) {
				if (job.AcceptCrew(crew))
					break;
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
        InvokeRepeating("UpdateJobs", 0f, 0.1f);
    }

    void UpdateJobs() {
        buildJobs.AssignJobs();
        foreach (var crew in ship.crew) {
            if (crew.job != null) {
                if (crew.job.isFinished) {
					Debug.LogFormat("Job finished: {0}", crew.job);
                    crew.job = null;
				} else
                    crew.job.Update();
            }
        }
    }
}
