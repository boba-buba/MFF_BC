// sig Wheel {} {
//     this in (Bicycle.front + Bicycle.rear)
// }

// sig Bicycle {
//     front: Wheel,
//     rear: Wheel
// }

// fact {
//     all b: Bicycle | b.rear != b.front
// }

// fun allWheels[b: Bicycle]: set Wheel {
//     b.front + b.rear
// }

// pred notIn[w: Wheel, b:Bicycle]
// {
//     w not in allWheels[b]
// }

// fact {
//     all b1, b2 : Bicycle | b1 != b2 =>
//         notIn[b1.front, b2] &&
//         notIn[b1.rear, b2]
// }


// run {} for 3 but 6 Wheel