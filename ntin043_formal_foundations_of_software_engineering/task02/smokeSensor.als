open util/integer

enum Time { T0, T1 }
enum SensorDetectionStatus {detected, nothing}


enum Action {StartFireAlarm, StopFireAlarm, TurnOnLight, TurnOffLight, Default}

abstract sig Device {
    var currAction : Action
}

abstract sig BoolSensor {
    var detected : SensorDetectionStatus
}

sig SmokeSensor extends BoolSensor {
}

sig SmokeController extends Device{
    ss : one SmokeSensor,
    var timer: Time
}

fun dt: Time {SmokeController.timer}
fun SensorDetectedSmth : SensorDetectionStatus {BoolSensor.detected}
fun currentAction : Action {Device.currAction}
fun smokeDetected : SensorDetectionStatus {SmokeController.ss.detected}

pred NoSmoke {
    -- preconditions
    dt = T0
    smokeDetected = nothing
    currentAction = Default
    -- postconditions
    dt' = T1
    currentAction' = Default
    -- frame conditions
    smokeDetected' = smokeDetected
}

pred SmokeStarted {
    -- preconditions
    dt = T1
    currentAction = Default
    smokeDetected = nothing
    -- postconditions
    dt' = T0
    smokeDetected' = detected
    currentAction' = StartFireAlarm
    -- frame conditions
}


pred AfteStartFireAlarm {
    -- preconditions
    dt = T0
    smokeDetected = detected
    currentAction = StartFireAlarm
    -- postconditions
    dt' = T1
    currentAction' = StopFireAlarm
    smokeDetected' = nothing
    -- frame conditions
}

pred AfterPutOutFire {
    -- preconditions
    dt != T0
    smokeDetected = nothing
    currentAction = StopFireAlarm
    -- postconditions
    dt' = T0
    currentAction' = Default
    smokeDetected' = nothing
    -- frame conditions
}

fact {
    dt = T0
    smokeDetected = nothing
    currentAction = Default
    always {NoSmoke or SmokeStarted or AfteStartFireAlarm or AfterPutOutFire}
}

run {} //for 3 but 1 SmokeController