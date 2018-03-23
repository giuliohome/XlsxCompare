module SqliteInterpreter
open XlsxDsl
open System.Data.SQLite

let createLogBook (dbName : string) = 
    SQLiteConnection.CreateFile(dbName)
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()
    let cmdDDL = 
        "create table xlsx_cols (" +
        "col_name VARCHAR(15), " +
        "col_index INT)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    conn.Close()
    conn.Dispose()


let createTable (dbName : string) (tableName: string) (colValNum: int) (keyType: string) (valType: string) =
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()
    
    let cmdDDL = 
        "create table \"" + tableName + "\" (" +
        "XlsxTag VARCHAR(15), " +
        "XlsxKey " + keyType + ", " + 
        "XlsxVal " + valType + ")"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdDDL = 
        "insert into xlsx_cols(col_name, col_index) " + 
        "values (@col_name, @col_index)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.Parameters.AddWithValue("@col_name",tableName) |> ignore
    cmd.Parameters.AddWithValue("@col_index",colValNum) |> ignore
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    conn.Close()
    conn.Dispose()

let readCollValues (h : Header)   : Map<string, string option> = 
    // TO DO
    Map.empty
