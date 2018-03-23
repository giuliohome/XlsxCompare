module SqliteInterpreter
open XlsxDsl
open System.Data.SQLite

let createLogBook (path : string) = 
    SQLiteConnection.CreateFile(path)


let readCollValues (h : Header)   : ColValues[] = 
    // TO DO
    [||]
