open util/integer
----------------------- ENUMS ---------------------------------
// Time to show different points in time
enum Time { T0, T1 }

// Detection types for BoolSensore type
enum SensorDetectionStatus {detected, nothing}

// Basic actions for sensor controllers
enum Action {StartFireAlarm, StopFireAlarm, TurnOnLight, TurnOffLight, Default}

// Indicator showing, whether thermostat is on or off
enum ThermostatStatus { On, Off }

// Types of image that intercom can get
enum Image {OwnerFace, Unknown}

// State of the door (to outside) indication, whether the house is open or not
enum DoorState {closed, opened}
----------------------- ENUMS ---------------------------------

------------------- ABSTRACT OBJECTS --------------------------
// Base entity for all sensors
abstract sig Sensor {}

// Numeric sensor, that gets measurements as numeric values
abstract sig NumSensor extends Sensor {
    var currentValue: Int,
    // Minimal temprature that sensor can detect
    minimum : Int,
    // Maximal temprature that sensor can detect
    maximum : Int,
} {
    currentValue >= minimum &&
    currentValue <= maximum &&
    minimum != maximum &&
    maximum > minimum
}

// Bool sensors that only detects whether smth happened or not
abstract sig BoolSensor extends Sensor {
    var detected : SensorDetectionStatus
}

// Generic device as a prototype of all sensor cotrollers
abstract sig Device {
    var currAction : Action
}
------------------- ABSTRACT OBJECTS --------------------------

------------------- SENSORS -----------------------------------

sig TempratureSensor extends NumSensor {
    // to which thermostat this sensor is connected
    respondsTo : this -> one Thermostat
} {
    // temprature sensor responds to only one thermostat
    one t : Thermostat | this -> t in respondsTo
    one t : Thermostat | this in t.ts
}


sig SmokeSensor extends BoolSensor {
    // to which smoke controller this sensor is connected
    respondsTo : this -> one SmokeController
} {
    // smoke sensor responds to only one Smoke controller
    one s : SmokeController | this -> s in respondsTo
    one s : SmokeController | this in s.ss
}

------------------- SENSORS -----------------------------------

------------------- CONTROLLERS -------------------------------
sig Thermostat extends Device {
    // timer for showing different periods of time
    var timer: Time,
    // every thermostat has only one temprature sensor
    ts : one TempratureSensor,
    // Thermostat state
    var ThermostatOn : ThermostatStatus,
} {
    #ts = 1
    one t: TempratureSensor | t in ts
}


sig SmokeController extends Device {
    ss : one SmokeSensor,
    // timer for showing different periods of time
    var timer: Time
}

------------------- CONTROLLERS -------------------------------

------------------- CONTROL UNIT ------------------------------

sig ControlUnit {
    devices : set Device
} {
    #devices > 0
}
------------------- CONTROL UNIT ------------------------------

---------------------- HOUSE ----------------------------------

// General room
sig Room {
    sensors: set Sensor,
}


fact {
    // Every room has at least one sensor
    all r : Room | #r.sensors > 0
    // Every sensor is in some room
    all s : Sensor | one r: Room | s in r.sensors
    // All devices are connected to exactly one ControlUnit
    all d : Device | one cu : ControlUnit | d in cu.devices
}


// Home structure
sig SmartHome {
    rooms : set Room,
    control: one ControlUnit,
    doors: set Door
} {
    // Number of rooms in every house is at least 2
    #rooms > 1
    // every house has 1 - 2 doors
    #doors > 0 && #doors < 3
}

fact {
    one SmartHome
    // every house has exactly one control unit
    all h : SmartHome | one cu : ControlUnit | cu in h.control
    // every door is a door in some house
    all d : Door | one h : SmartHome | d in h.doors
    // every room is a room in some house
    all r : Room | one h : SmartHome | r in h.rooms
    // every control unit is in some house
    all cu : ControlUnit | one h : SmartHome | cu in h.control
}


---------------------- HOUSE ----------------------------------

------------------- INTERCOM AND DOOR -------------------------

// Intercom device with video camera
sig Intercom {
    var currentImage : Image,
    var timer : Time
}

sig Door {
    intercom : one Intercom,
    var state : DoorState
}

------------------- INTERCOM AND DOOR -------------------------

------------------- STATES SIMULATION FOR DOOR ----------------

// Current image from the intercom
fun GetCurrentImage: Image {Intercom.currentImage}

// Closed/opened door at the moment
fun doorState : DoorState {Door.state}

// Intercom in different periods of time
fun iT: Time {Intercom.timer}

pred DoorOpened {
    -- pre
    iT = T0
    doorState = closed
    GetCurrentImage = Unknown
    -- post
    iT' = T1
    doorState' = opened
    GetCurrentImage' = OwnerFace
    -- frame
}

pred DoorClosed {
    -- pre
    iT != T0
    doorState = opened
    GetCurrentImage = OwnerFace
    -- post
    iT' = T0
    doorState' = closed
    GetCurrentImage' = Unknown
    -- frame
}

// Initial configuration for Door
fact {
    iT = T0
    doorState = closed
    GetCurrentImage = Unknown
    always (DoorOpened or DoorClosed)
}
------------------- STATES SIMULATION FOR DOOR ----------------

------------------- STATES SIMULATION FOR THERMOSTAT ----------

// Thermostat status at the current moment
fun thOn : ThermostatStatus {Thermostat.ThermostatOn}

// Thermstat in different periods of time
fun t : Time {Thermostat.timer}

// Current tempruture measured by tempr. sensor
fun currentTemp : Int {Thermostat.ts.currentValue}

// Maximum possible temprature for the thermostat to set
fun tempMinimum : Int {Thermostat.ts.minimum}

// Minimum possible temprature for the thermostat to set
fun tempMaximum : Int {Thermostat.ts.maximum}

// Indicator, whether sensor detected smth
fun SensorDetectedSmth : SensorDetectionStatus {BoolSensor.detected}
fun currThermAction : Action {Thermostat.currAction}

pred ThermostatOnState {
    -- preconditions
    t = T0
    currentTemp = tempMinimum
    thOn = Off
    -- postconditions
    thOn' = On
    currentTemp' > tempMinimum
    t' = T1
    -- frame conditions
    currentTemp' < tempMaximum
    currThermAction' = currThermAction
}

pred ThermostatOffState {
    -- precondtions
    t = T0
    currentTemp = tempMaximum
    thOn = On
    -- postconditions
    t' = T1
    currentTemp' < tempMaximum
    thOn' = Off
    -- frame conditions
    currentTemp' > tempMinimum
    currThermAction' = currThermAction
}

pred ThermostatSteadyState {
    -- preconditions
    t != T0
    currentTemp > tempMinimum
    currentTemp < tempMaximum
    -- postconditions
    t' = T0
    currentTemp' != currentTemp
    -- frame conditions
    thOn' = thOn
    currThermAction' = currThermAction
}

// Initial configuration for thermostat
fact {
    thOn = Off
    t = T0
    currentTemp = tempMinimum
    currThermAction = Default
    always (ThermostatOnState or ThermostatOffState or ThermostatSteadyState)
}

------------------- STATES SIMULATION FOR THERMOSTAT ----------

------------- STATES SIMULATION FOR SMOKE CONTROLLER ----------
fun dt: Time {SmokeController.timer}
fun SensorDetectedSmth : SensorDetectionStatus {BoolSensor.detected}
fun smokeDetected : SensorDetectionStatus {SmokeController.ss.detected}
fun currentAction : Action {SmokeController.currAction}

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

// Initial state of the Smoke controller
fact {
    dt = T0
    smokeDetected = nothing
    currentAction = Default
    always {NoSmoke or SmokeStarted or AfteStartFireAlarm or AfterPutOutFire}
}

------------- STATES SIMULATION FOR SMOKE CONTROLLER ----------

--------------------- CHECKS AND ASSERTIONS -------------------
assert SensorInRoom {
    // Eevry sensor must be in exactly one room
    all s : Sensor | one r : Room | s in r.sensors
}

// As we are running for only one house several conditions must pass
assert CorrectNumbers {
    // Instance must not have mor than two doors
    #Door < 3
    // There must be only one control unit in instance, because in one house has only one control unit
    #ControlUnit = 1
    // Devices are controllers that collect measures from sensors. One device can collect measures from several sensors, but sensor sends measures to only one controller.
    #Device <= #Sensor
}

check CorrectNumbers for 3

check SensorInRoom for 2

--------------------- CHECKS AND ASSERTIONS -------------------

run {} for 3
