enum Time { T0, T1 }
enum Image {OwnerFace, Unknown}
enum DoorState {closed, opened}

sig Intercom {
    var currentImage : Image,
    var timer : Time
}

sig Door {
    intercom : one Intercom,
    var state : DoorState
}

fun GetCurrentImage: Image {Intercom.currentImage}
fun doorState : DoorState {Door.state}
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

pred DoorClosedNothing {
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

fact doorInOneState {
    iT = T0
    doorState = closed
    GetCurrentImage = Unknown
    always (DoorOpened or DoorClosedNothing)
}

run {} for 3 but 1 Door