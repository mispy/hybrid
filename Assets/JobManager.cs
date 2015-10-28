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
    public readonly Block targetBlue;

    public BuildJob(Block block) {
        this.targetBlue = block;
    }

	// if none of the neighbors are passable then nobody can build this so we
	// shouldn't bother checking for assignments
	public bool IsPossible() {
        if (targetBlue.ship == null) {
            isFinished = true;
            return false;
        }

        if (targetBlue.ship.blocks.IsOutsideBounds(targetBlue.pos))
            return false;

		bool canAttach = IntVector2.Neighbors(targetBlue.pos).Any((n) => targetBlue.ship.blocks[n, BlockLayer.Base] != null);
        if (!canAttach) return false;
		bool canAccess = IntVector2.NeighborsWithDiagonal(targetBlue.pos).Any((n) => targetBlue.ship.blocks.IsPassable(n));
        if (!canAccess) return false;

        return true;
	}

    public bool AcceptCrew(Crew crew) {
        var neighbors = IntVector2.NeighborsWithDiagonal(targetBlue.pos).OrderBy((n) => IntVector2.Distance(crew.body.currentBlockPos, n));
        foreach (var neighbor in neighbors) {
            if (crew.mind.CanReach(neighbor)) {
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

        if (BlueprintBlock.Matches(targetBlue, targetBlue.ship.blocks[targetBlue.pos, targetBlue.layer])) {
			Debug.LogFormat("Successfully built: {0}", targetBlue);
            isFinished = true;
            return;
        }

        if (IntVector2.NeighborsWithDiagonal(targetBlue.pos).Contains(crew.body.currentBlockPos)) {
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

	public struct JobPair {
		public Crew crew;
		public BuildJob job;
		public double dist;
	}

    public BuildJobManager(Ship ship) {
        this.ship = ship;
        ship.blueprintBlocks.OnBlockAdded += OnBlueprintAdded;
		foreach (var block in ship.blueprintBlocks.allBlocks) {
			OnBlueprintAdded(block);
		}
    }

    void OnBlueprintAdded(Block block) {

        if (block != null && block.ship.blocks[block.pos, block.layer] == null)
	        jobs.Add(new BuildJob(block));
    }

    public void AssignJobs() {
        jobs = jobs.Where((j) => j.isFinished == false).ToList();

		var unassignedCrew = ship.crew.Where((crew) => crew.job == null);

		var pairs = new List<JobPair>();

		foreach (var job in unassignedJobs) {
			if (!job.IsPossible())
				continue;

			foreach (var crew in unassignedCrew) {
				var pair = new JobPair();
				pair.crew = crew;
				pair.job = job;
				pair.dist = IntVector2.Distance(crew.body.currentBlockPos, job.targetBlue.pos);
				pairs.Add(pair);
			}
		}

		foreach (var pair in pairs.OrderBy((p) => p.dist)) {
			if (pair.crew.job == null) {
				pair.crew.job = pair.job;
			}
		}
    }
}

public class JobManager : MonoBehaviour {
    Ship ship;
    BuildJobManager buildJobs;

    void OnEnable() {
        ship = GetComponentInParent<Blockform>().ship;
        buildJobs = new BuildJobManager(ship);
        InvokeRepeating("UpdateJobs", 0f, 0.01f);

    }

    void UpdateJobs() {

        buildJobs.AssignJobs();
        foreach (var crew in ship.crew) {
            if (crew.job != null) {
                if (crew.job.isFinished) {
					//Debug.LogFormat("Job finished: {0}", crew.job);
                    crew.job = null;
				} else {
                    crew.job.Update();
				}
            }
        }
    }
}
