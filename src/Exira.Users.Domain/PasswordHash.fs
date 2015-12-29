namespace Exira.Users.Domain

// https://crackstation.net/hashing-security.htm

[<AutoOpen>]
module PasswordHash =
    open System
    open System.Security.Cryptography
    open Chiron

    type PasswordHash = PasswordHash of string
    let apply f (PasswordHash e) = f e
    let value e = apply id e

    let [<Literal>] private SaltByteSize = 24
    let [<Literal>] private HashByteSize = 24
    let [<Literal>] private PBKDF2Iterations = 20000
    let [<Literal>] private IterationIndex = 0
    let [<Literal>] private SaltIndex = 1
    let [<Literal>] private PBKDF2Index = 2

    let inline private slowEquals xs ys =
        let equals (x: byte []) (y: byte [])  =
            let mutable diff = uint64 x.Length ^^^ uint64 y.Length

            let mutable i = 0
            while i < x.Length && i < y.Length do
                diff <- diff ||| uint64 x.[i] ^^^ uint64 y.[i]
                i <- i + 1

            diff = 0uL

        match xs, ys with
        | null, null
        | [||], [||] -> failwith "both input arrays were null or empty"

        | null, _
        | _   , null
        | [||], _
        | _   , [||] -> failwithf "one input array was null or empty"

        | x, y -> equals x y

    let private pbkdf2 password (salt: byte[]) iterations outputBytes =
        use pbkdf2 = new Rfc2898DeriveBytes(Password.value password, salt, iterations)
        pbkdf2.GetBytes outputBytes

    let validatePassword password correctHash =
        let correctHash = value correctHash

        let delimiter = [| ':' |]
        let split = correctHash.Split delimiter
        let iterations = Int32.Parse split.[IterationIndex]
        let salt = Convert.FromBase64String split.[SaltIndex]
        let hash = Convert.FromBase64String split.[PBKDF2Index]
        let testHash = pbkdf2 password salt iterations hash.Length

        slowEquals hash testHash

    let createHash password =
        use csprng = new RNGCryptoServiceProvider()
        let salt: byte [] = Array.zeroCreate SaltByteSize
        csprng.GetBytes salt

        let hash = pbkdf2 password salt PBKDF2Iterations HashByteSize

        let salt = Convert.ToBase64String(salt)
        let hash = Convert.ToBase64String(hash)
        sprintf "%i:%s:%s" PBKDF2Iterations salt hash |> PasswordHash

    let toJson (hash: PasswordHash) =
        hash
        |> value
        |> String

    let fromJson json =
        let error x =
            Json.formatWith JsonFormattingOptions.SingleLine x
            |> sprintf "couldn't deserialise to PasswordHash: %s"
            |> Error

        match json with
        | String hash -> hash |> PasswordHash |> Value
        | _ -> error json