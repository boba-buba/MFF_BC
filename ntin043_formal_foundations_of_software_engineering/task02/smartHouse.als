// open util/integer
// // option A: smart home
// //       - important aspects: various sensors, control, security and other equipment
// //       - you have to decide what entities and operations to capture in the model
// //           - example: temperature (heating), smoke, lights, cameras, movement, automated locking
// //           - example: automatically turn on/off some devices based on values recorded by sensors
// //       - do not forget to define some assertions (facts) and commands (run, check)

// //various sensors, control, security and other equipment

// enum Time { T0, T1 }
// enum ThermostatStatus { On, Off }
// // Sensor detected smth
// enum SensorDetectionStatus {detected, nothing}

// enum General {Yes, No}

// abstract sig Sensor {
//     var currentValue: Int,
//     minimum : Int,
//     maximum : Int,
//     var detected : SensorDetectionStatus
// } {
//     currentValue >= minimum &&
//     currentValue <= maximum &&
//     minimum != maximum &&
//     maximum > minimum
// }

// enum Action {StartFireAlarm, StopFireAlarm, TurnOnLight, TurnOffLight, Default}


// abstract sig Device {
//     var currAction : Action
// }


// sig TempratureSensor extends Sensor {}


// sig Thermostat extends Device {
//     var timer: Time,
//     ts : one TempratureSensor,
//     var ThermostatOn : ThermostatStatus,
// }

// fun thOn : ThermostatStatus {Thermostat.ThermostatOn}
// fun t : Time {Thermostat.timer}

// fun dt: Time {SmokeController.timer}
// fun currentTemp : Int {Thermostat.ts.currentValue}
// fun tempMinimum : Int {Thermostat.ts.minimum}
// fun tempMaximum : Int {Thermostat.ts.maximum}

// fun SensorDetectedSmth : SensorDetectionStatus {Sensor.detected}
// fun currentAction : Action {Device.currAction}
// fun smokeDetected : SensorDetectionStatus {SmokeController.ss.detected}

// pred ThermostatOnState {
//     -- preconditions
//     t = T0
//     currentTemp = tempMinimum
//     thOn = Off
//     -- postconditions
//     thOn' = On
//     currentTemp' > tempMinimum
//     t' = T1
//     -- frame conditions
//     currentTemp' < tempMaximum
// }

// pred ThermostatOffState {
//     -- precondtions
//     t = T0
//     currentTemp = tempMaximum
//     thOn = On
//     -- postconditions
//     t' = T1
//     currentTemp' < tempMaximum
//     thOn' = Off
//     -- frame conditions
//     currentTemp' > tempMinimum
// }

// pred ThermostatSteadyState {
//     -- preconditions
//     t != T0
//     currentTemp > tempMinimum
//     currentTemp < tempMaximum
//     -- postconditions
//     t' = T0
//     currentTemp' != currentTemp
//     -- frame conditions
//     thOn' = thOn
// }

// fact {
//     thOn = Off
//     t = T0
//     currentTemp = tempMinimum
//     always (ThermostatOnState or ThermostatOffState or ThermostatSteadyState)
// }

// //Running thermostat
// //run {} for 3 but 1 Thermostat

// sig SmokeSensor extends Sensor {
//     var timer: Time,
// }

// sig SmokeController {
//     ss : one SmokeSensor,
//     fromRoom : Room -> Sensor
// } {
//     all s : SmokeSensor, r : Room | (s in ss) implies (r->s in fromRoom)
// }

// // pred NoSmoke {
// //     -- preconditions
// //     dt = T0
// //     smokeDetected = nothing
// //     //currentAction = StopFireAlarm
// //     -- postconditions
// //     dt' = T1
// //     smokeDetected' = smokeDetected
// //     //currentAction' = Default
// //     -- frame conditions
// //     // smokeDetected' = smokeDetected
// //     // currentAction' = currentAction
// // }

// // pred SmokeStarted {
// //     -- preconditions
// //     dt = T0
// //     //currentAction = Default
// //     smokeDetected = nothing
// //     -- postconditions
// //     dt' = T1
// //     smokeDetected' = detected
// //     //currentAction' = StartFireAlarm
// //     -- frame conditions
// // }


// // pred AfteStartFireAlarm {
// //     -- preconditions
// //     dt = T0
// //     smokeDetected = detected
// //     //currentAction = StartFireAlarm
// //     -- postconditions
// //     dt' = T1
// //     //currentAction' = StopFireAlarm
// //     smokeDetected' = nothing
// //     -- frame conditions
// // }

// // pred AfterPutOutFire {
// //     -- preconditions
// //     t = T0
// //     smokeDetected = detected
// //     currentAction = StopFireAlarm
// //     -- postconditions
// //     t' = T1
// //     currentAction' = Default
// //     smokeDetected' = nothing
// //     -- frame conditions
// // }

// // fact {
// //     dt = T0
// //     //smokeDetected = detected
// //     //currentAction = Default
// //     always {NoSmoke or SmokeStarted or AfteStartFireAlarm }
// // }

// // run {} //for 3 but 1 SmokeController

// // sig MotionSensor extends Sensor {

// // }

// sig ControlUnit extends Device {
//     devices : set Device

// }

// abstract sig Room {
//     sensors: set Sensor,
// } {
//     one s: Sensor | s = TempratureSensor => s in sensors
// }

// sig Kitchen extends Room {
//     smokeSensor: one SmokeSensor
// } {

// }



// // ////////Intercom and Door////////
// enum Image {OwnerFace, UnknownFace, NoFace}

// sig Intercom {
//     currentImage : Image
// }

// enum DoorState {closed, opened}

// sig Door {
//     intercom : Intercom,
//     state : DoorState
// }

// fun GetCurrentImage[i: Intercom]: one Image {
//     i.currentImage
// }

// pred DoorOpened {
//     all d: Door | GetCurrentImage[d.intercom] = OwnerFace && d.state = opened
// }

// pred DoorClosed {
//     all d: Door | GetCurrentImage[d.intercom] != OwnerFace && d.state = closed
// }

// fact doorInOneState {
//    always (DoorClosed or DoorOpened)
// }

// // ////////Intercom and Door////////


// // sig SmartHome {
// //     rooms : set Room,
// //     // control: one ControlUnit,
// //     door: one Door
// //  }

// // fact {
// //     one SmartHome
// // }

// // fun AllSensorsInRoom[r: Room]: set Sensor {
// //     //r.tempSensor + r.motionSensor
// //     r.sensors
// // }

// // pred NotIn[s: Sensor, r: Room] {
// //     s not in AllSensorsInRoom[r]
// // }

// // // fact {
// // //     all r1, r2 : Room | r1 != r2 =>
// // //         NotIn[r1.motionSensor, r2] &&
// // //         NotIn[r1.tempSensor, r2]
// // // }

// // fact {
// //     // Every sensor is in some room
// //     all s: Sensor | one r: Room | s in r.sensors

// //     all r1, r2: Room | r1.sensors not in r2.sensors => r2.sensors not in r1.sensors
// //     // Every room in some house
// //     all r: Room | one h: SmartHome | r in h.rooms
// //     // Every house has at most one living room
// //     all h: SmartHome | lone lr: LivingRoom | lr in h.rooms

// //     // Every room has at least 2 sensors
// //     all r: Room | #r.sensors >= 2
// //     // // Every house has at least one bathroom
// //     // all h: SmartHome | some bath: Bathroom | h.rooms lone -> some bath

// // }

// //run {} for 3// but 1 Door, 1 Intercom, 2 TempratureSensor