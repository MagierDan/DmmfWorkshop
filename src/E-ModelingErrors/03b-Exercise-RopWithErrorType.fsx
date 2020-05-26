﻿(*
Railway oriented programming -- with custom error type
*)

// IMPORTANT - if you get errors such as
//   Could not load type 'ErrorMessage' from assembly 'FSI-ASSEMBLY,
// then try:
// 1. Reset the FSI interactive
// 2. Load code in small chunks

// Load a file with library functions for Result
#load "Result.fsx"

// Some data to validate
type Request = {
    UserId: int
    Name: string
    Email: string
}

//===========================================
// Define the error type here.
// Add to it incrementally as you develop the pipeline.
//===========================================

type ErrorMessage =
  | NameMustNotBeBlank   // name not blank
  | NameMustNotBelongerThan of int  // name not longer than
  | EmailMustNotBeBlank  // email not longer than


//===========================================
// A library of utility functions for railway oriented programming
// which are not specific to this workflow and could be reused.
//===========================================

module RopUtil =

    /// convert a "dead-end" function into a useful function
    let tee f result =
      f result
      result

    /// convert an exception throwing function
    /// into a useful function
    let catch exceptionThrowingFunction handler oneTrackInput =
        try
            Ok (exceptionThrowingFunction oneTrackInput)
        with
        | ex ->
            Error (handler ex)

    /// like catch but with *twoTrackInput*
    let catchR exceptionThrowingFunction handler twoTrackInput =
        let catch' = catch exceptionThrowingFunction handler
        twoTrackInput |> Result.bind catch'

//===========================================
// Step 1 of the pipeline: validation
//===========================================

let nameNotBlank input =
  if input.Name = "" then
    Error NameMustNotBeBlank
  else
    Ok input

let name50 input =
  if input.Name.Length > 50 then
    Error (NameMustNotBelongerThan 50)
  else
    Ok input

let emailNotBlank input =
  if input.Email = "" then
    Error EmailMustNotBeBlank
  else
    Ok input

/// Combine all the smaller validation functions into one big one
let validateRequest input =
  input
  |> nameNotBlank
  |> Result.bind name50
  |> Result.bind emailNotBlank

// -------------------------------
// test the "validateRequest" step interactively
// before implementing the next step

let goodRequest = {
  UserId=0
  Name= "Alice"
  Email="ABC@gmail.COM"
}
goodRequest |> validateRequest

let badRequest1 = {
  UserId=0
  Name= ""
  Email="abc@example.com"
}
badRequest1 |> validateRequest

let unsendableRequest = {
  UserId=0
  Name= "Alice"
  Email="ABC@example.COM"
}
unsendableRequest |> validateRequest

//===========================================
// Step 2 of the pipeline: lowercasing the email
//===========================================

// trim spaces and lowercase
let canonicalizeEmail input =
   { input with Email = input.Email.Trim().ToLower() }

let canonicalizeEmailR twoTrackInput =  // value restriction error fixed!!!
  twoTrackInput |> Result.map canonicalizeEmail

// -------------------------------
// test the "canonicalize" step interactively
// before implementing the next step

goodRequest
|> validateRequest
|> canonicalizeEmailR

//===========================================
// Step 3 of the pipeline: Update the database
//===========================================

let updateDb (request:Request) =
    // do something
    // return nothing at all
    printfn "Database updated with userId=%i email=%s" request.UserId request.Email
    ()

let updateDbR twoTrackInput =
  twoTrackInput |> Result.map (RopUtil.tee updateDb)

// -------------------------------
// test the "updateDbR" step interactively
// before implementing the next step

goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR


//===========================================
// Step 4 of the pipeline: Send an email
//===========================================

let sendEmail (request:Request) =
    if request.Email.EndsWith("example.com") then
        failwithf "Can't send email to %s" request.Email
    else
        printfn "Sending email=%s" request.Email
        request // return request for processing by next step

let sendEmailR twoTrackInput =
    // convert SMTP exceptions to our list
    let handler (ex:exn) = SmtpServerError ex.Message
    RopUtil.catchR sendEmail handler twoTrackInput

// -------------------------------
// test the "sendEmailR" step interactively
// before implementing the next step

goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR

unsendableRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR

//===========================================
// Step 5 of the pipeline: Log the errors
//===========================================

let loggerR twoTrackInput =
    match twoTrackInput with
    | Ok (req:Request) ->
        printfn "LOG INFO Name=%s EMail=%s" req.Name req.Email
    | Error err ->
        printfn "LOG ERROR %A" err
    twoTrackInput

// -------------------------------
// test the "loggerR" step interactively
// before implementing the next step

goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR
|> loggerR

//===========================================
// Translator from error type to string
//===========================================

// obviously the real ones would use resource files!

let translateError_EN err =
    match err with
    | ?? ->
        "Name must not be blank"
    | ?? i ->
        sprintf "Name must not be longer than %i chars" i
    | ?? ->
        "Email must not be blank"
    | SmtpServerError msg ->
        sprintf "SmtpServerError [%s]" msg

let translateError_FR err =
    match err with
    | ?? ->
        "Nom ne doit pas être vide"
    | ?? i ->
        sprintf "Nom ne doit pas être plus long que %i caractères" i
    | ?? ->
        "Email doit pas être vide"
    | SmtpServerError msg ->
        sprintf "SmtpServerError [%s]" msg

//===========================================
// Last step of the pipeline: return the response
//===========================================


let returnMessageR translator result =
    match result with
    | Ok obj ->
        sprintf "200 %A" obj
    | Error msg ->
        let errStr = translator msg
        sprintf "400 %s" errStr

// -------------------------------
// test the "returnMessageR" step interactively
goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR
|> loggerR
|> returnMessageR translateError_EN


//===========================================
// Finally, build a bigger function that runs the whole pipeline
//===========================================

let updateCustomerR request =
  request
  |> validateRequest
  |> canonicalizeEmailR
  |> updateDbR
  |> sendEmailR
  |> loggerR
  |> returnMessageR translateError_FR


// -------------------------------
// test the entire pipeline

goodRequest |> updateCustomerR

badRequest1 |> updateCustomerR

unsendableRequest |> updateCustomerR


