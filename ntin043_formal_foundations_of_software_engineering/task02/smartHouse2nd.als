open util/integer

//enum Time { T0, T1 }
//enum ThermostatStatus { On, Off }
// Sensor detected smth
//enum SensorDetectionStatus {detected, nothing}

//enum General {Yes, No}

abstract sig Sensor {
    // var currentValue: Int,
    // minimum : Int,
    // maximum : Int,
    //var detected : SensorDetectionStatus
} {
    // currentValue >= minimum &&
    // currentValue <= maximum &&
    // minimum != maximum &&
    // maximum > minimum
}

//enum Action {StartFireAlarm, StopFireAlarm, TurnOnLight, TurnOffLight, Default}


abstract sig Device {
    //var currAction : Action
}


sig TempratureSensor extends Sensor {
    respondsTo : this -> one Thermostat
} {
    #respondsTo = 1
    one t: Thermostat | this -> t in respondsTo && this in t.ts
}


sig Thermostat extends Device {
    //var timer: Time,
    ts : set TempratureSensor,
    //var ThermostatOn : ThermostatStatus,
}
{
    one r : Room | one s : TempratureSensor | s in ts && s in r.sensors
    //thermosta is always in some controlunit
    one cu : ControlUnit | this in cu.devices
}

// sig SmokeSensor extends Sensor {
//     //var timer: Time,
// } {

// }

// sig SmokeController extends Device {
//     ss : one SmokeSensor,
// }
// {
//    one r : Room | r->ss in r.fromRoom
// }

sig ControlUnit extends Device {
    devices : set Device
}

sig Room {
    sensors: set Sensor,
    //fromRoom : Room -> Sensor
 } 
 //{
//     //all s1, s2 : Sensor | (s1 in sensors && s2 in sensors) implies s1 != s2
// }

fact {
    #Room > 0
    all r : Room | #r.sensors > 0
    all s : Sensor | one r: Room | s in r.sensors
    //all s : Sensor, r1, r2 : Room | (s in r1.sensors) => ( s not in r2.sensors)
    one ControlUnit
    all d : Device | one cu : ControlUnit | d in cu.devices
}


run {} for 3
