﻿/// =============================================
/// Convert a number to Roman
/// =============================================

(*

Use the "tally" system:

* start with n "I"
* replace "IIIII" with "V"
* replace "VV" with "X"
* replace "XXXXX"  with "L"
* replace "LL" with "C"
* replace "CCCCC"  with "D"
* replace "DD" with "M"

Challenge, write this using a piping model with partial application.
Use the code below as a starting point

For extra points, handle IV, IX, XC, etc.

*)

/// Convert the built-in .NET library method
/// to a pipeable function
/// (automatic currying)
let replace (oldValue:string) (newValue:string) (inputStr:string) =
    inputStr.Replace( oldValue=oldValue, newValue=newValue)


// uncomment this code to start
let toRomanNumerals number =
    let replace_IIIII_V str = replace "IIIII" "V" str
    let replace_VV_X str = replace "VV" "X" str
    let replace_XXXXX_L str = replace "XXXXX" "L" str
    let replace_LL_C str = replace "LL" "C" str
    let replace_CCCCC_D str = replace "CCCCC" "D" str
    let replace_DD_M str = replace "DD" "M" str

    String.replicate number "I"
    |> replace_IIIII_V
    |> replace_VV_X
    |> replace_XXXXX_L
    |> replace_LL_C
    |> replace_CCCCC_D
    |> replace_DD_M

// test it
toRomanNumerals 12
toRomanNumerals 14
toRomanNumerals 1947

(*
The replace function can also be used "inline".
To do this, pass the first two parameters explicitly, and the last parameter
will be passed implicitly via the pipe

The advantage of this approach is that you don't need to define all the helper functions
*)


// Inline version
let toRomanNumerals_v2 number =
    String.replicate number "I"
    |> replace "IIIII" "V"
    |> replace "VV" "X"
    |> replace "XXXXX" "L"
    |> replace "LL" "C"
    |> replace "CCCCC" "D"
    |> replace "DD" "M"

// test it
toRomanNumerals_v2 12
toRomanNumerals_v2 14
toRomanNumerals_v2 1947


(*
What about the special forms IV,IX,XC etc?

Exercise: add these as additional transforms at the end of the pipe
Just replace "IIII" with "IV", etc
*)

let toRomanNumerals_v3 number =
    String.replicate number "I"
    |> replace "IIIII" "V"
    |> replace "VV" "X"
    |> replace "XXXXX" "L"
    |> replace "LL" "C"
    |> replace "CCCCC" "D"
    |> replace "DD" "M"

    // additional special forms
    |> replace "VIIII" "IX"
    |> replace "IIII" "IV"
    |> replace "LXXXX" "XC"
    |> replace "XXXX" "XL"

// test it
toRomanNumerals_v3 4
toRomanNumerals_v3 14
toRomanNumerals_v3 19

