using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State {
    Idle,
    Moving,
    Combat,
    Retreat
}

public enum Affiliation {
    Blue,
    Red,
    Green
}

public enum TaskForceBehaviour {
    Passive,
    Aggresive,
    Evasive
}

public enum TaskForceOrder {
    None,
    Engage,
    Disengage,
    Move,
    Patrol,
    Defend
}
