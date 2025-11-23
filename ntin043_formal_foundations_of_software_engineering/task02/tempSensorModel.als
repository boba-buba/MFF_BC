open util/integer

enum Time { T0, T1 }
enum ThermostatStatus { On, Off }
enum SensorDetectionStatus {detected, nothing}


abstract sig Sensor {
    var currentValue: Int,
    minimum : Int,
    maximum : Int,
    var detected : SensorDetectionStatus
} {
    currentValue >= minimum &&
    currentValue <= maximum &&
    minimum != maximum &&
    maximum > minimum
}

enum Action {StartFireAlarm, StopFireAlarm, TurnOnLight, TurnOffLight, Default}


abstract sig Device {
    var currAction : Action
}


sig TempratureSensor extends Sensor {}


sig Thermostat extends Device {
    var timer: Time,
    ts : one TempratureSensor,
    var ThermostatOn : ThermostatStatus,
}

fun thOn : ThermostatStatus {Thermostat.ThermostatOn}
fun t : Time {Thermostat.timer}

fun currentTemp : Int {Thermostat.ts.currentValue}
fun tempMinimum : Int {Thermostat.ts.minimum}
fun tempMaximum : Int {Thermostat.ts.maximum}

fun SensorDetectedSmth : SensorDetectionStatus {Sensor.detected}
fun currentAction : Action {Device.currAction}

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
}

fact {
    thOn = Off
    t = T0
    currentTemp = tempMinimum
    always (ThermostatOnState or ThermostatOffState or ThermostatSteadyState)
}

//Running thermostat
run {} for 3 but 1 Thermostat
