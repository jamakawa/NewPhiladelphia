var animationSpeed:float;

function Update () {
    for (var state : AnimationState in GetComponent.<Animation>()) {
        state.speed = animationSpeed;
    }
}
