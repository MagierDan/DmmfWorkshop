// ================================================
// Decorator pattern in FP
// ================================================

// Create an input log function and an output log function
// and then use them to create a "logged" version of add1


let add1 x = x + 1

// test
add1 4
[1..10] |> List.map add1   // with a list


// ===========================================
// define the logging functions
// ===========================================

let logTheInput (x:int) =
    printfn "%i" x
    x  // use printfn

let logTheOutput (x:int) =
    printfn "%i" x
    x  // use printfn


// ===========================================
// define the logged version of add1
// ===========================================

// TIP for add1Logged use piping "|>"
let add1Logged x =
    x |> logTheInput |> add1 |> logTheOutput

// test
add1Logged 4
[1..10] |> List.map add1Logged