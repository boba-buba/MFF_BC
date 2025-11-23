// // Signatures:
// sig A {}

// // sig B {
// //     parent : A
// // }
// // Predicates and facts
// pred atLeastOne {
//     #A > 0
// }

// pred notAsingleOne {
//     #A != 1
// }

// //applied on model
// fact moreThenOne {
//     atLeastOne
//     notAsingleOne
// }
// fact {
//     #A > 2
// }
// assert greaterThanTwo {
//     #A > 2
// }

// check greaterThanTwo