using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CrewTask {
    public CrewBody crew;
    public virtual CrewBody ChooseCrew(IEnumerable<CrewBody> availCrew) {
        return availCrew.First();
    }
    public virtual void Update() {}
    public virtual bool Equals(CrewTask task) {
        return this == task;
    }
}

public class RepairTask : CrewTask {
    Block damagedBlock;

    public RepairTask(Block block) {
        this.damagedBlock = block;
    }

    public override bool Equals(CrewTask task) {
        return task is RepairTask && ((RepairTask)task).damagedBlock == damagedBlock;
    }

    public override CrewBody ChooseCrew(IEnumerable<CrewBody> availCrew) {
        var ordered = availCrew.OrderBy((crew) => IntVector2.Distance(crew.currentBlockPos, damagedBlock.pos));
        return ordered.First();
    }

    public override void Update() {
        if (crew.mind.blockPath.Count > 0) return;

        if (IntVector2.Distance(crew.currentBlockPos, damagedBlock.pos) <= RepairTool.range) {
            crew.repairTool.Repair(damagedBlock.worldPos);
        } else {
            foreach (var neighbor in IntVector2.NeighborsWithDiagonal(damagedBlock.pos)) {
                if (damagedBlock.ship.blocks.IsPassable(neighbor)) {
                    crew.mind.SetMoveDestination(neighbor);
                }
            }
        }
    }
}

public class ConsoleTask : CrewTask {
    Console console;

    public ConsoleTask(Console console) {
        this.console = console;   
    }

    public override CrewBody ChooseCrew(IEnumerable<CrewBody> availCrew) {
        var ordered = availCrew.OrderBy((crew) => IntVector2.Distance(crew.currentBlockPos, console.block.pos));
        return ordered.First();
    }

    public override void Update() {
        if (crew.mind.blockPath.Count > 0) return;

        if (crew.currentBlock == console.block && (console.crew == null || console.crew.mind != null)) {
            console.crew = crew;
        } else {
            crew.mind.SetMoveDestination(console.block.pos);   
        }
    }
}

public class CrewTaskManager : MonoBehaviour {
    public Blockform ship;
    public List<CrewTask> tasks = new List<CrewTask>();

    void Awake() {
        ship = GetComponentInParent<Blockform>();
    }

    void Start() {
        InvokeRepeating("RecalcTasks", 0f, 0.5f);
    }

    void Update() {
        foreach (var task in tasks) {
            if (task.crew != null)
                task.Update();
        }
    }

    void AddTask(CrewTask task) {
        foreach (var currentTask in tasks) {
            if (currentTask.Equals(task))
                return;
        }

        tasks.Add(task);
    }

    public void RecalcTasks() {
        tasks.Clear();

        // Repairing the reactor is a top priority
        foreach (var reactor in ship.blocks.Find<Reactor>()) {
            if (reactor.isDestroyed) {
                AddTask(new RepairTask(reactor));
            }
        }

        // For consoles, the priority is to repair everything connected to them
        // first and then find a crew member to staff them
        foreach (var consoleBlock in ship.blocks.Find<Console>()) {
            if (consoleBlock.isDestroyed) {
                AddTask(new RepairTask(consoleBlock));
                continue;
            }

            var console = consoleBlock.GetBlockComponent<Console>();
            foreach (var block in console.connectedBlocks) {
                if (block.isDestroyed)
                    AddTask(new RepairTask(block));
            }

            if (console.crew == null || console.crew.mind != null)
                AddTask(new ConsoleTask(console));
        }           

        // If there's nothing else to do, repair the rest of the ship
        foreach (var block in ship.blocks.allBlocks) {
            if (block.isDamaged)
                AddTask(new RepairTask(block));
        }

        AssignTasks();
    }

    public void AssignTasks() {
        var availCrew = ship.friendlyCrew.Where((crew) => crew.mind != null).ToList();
        foreach (var task in tasks) {
            if (availCrew.Count == 0) break;
            var crew = task.ChooseCrew(availCrew);
            availCrew.Remove(crew);
            task.crew = crew;
            //Debug.LogFormat("Assigning {0} to {1}", crew, task);
        }
    }
}
