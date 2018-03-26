module SqliteInterpreter
open XlsxDsl
open System.Data.SQLite
open System

let createLogBook (dbName : string) (keyName : string) (keyType : string)  (keyCol : int)= 
    SQLiteConnection.CreateFile(dbName)
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()

    let cmdDDL = 
        "create table xlsx_cols (" +
        "col_name VARCHAR(15), " +
        "col_type VARCHAR(10), " +
        "col_index INT)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdDDL = 
        "create table xlsx_key (" +
        "col_name VARCHAR(15), " +
        "col_type VARCHAR(10), " +
        "col_index INT)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdDDL = 
        "insert into xlsx_key(col_name, col_type, col_index) " + 
        "values (@col_name, @col_type, @col_index)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.Parameters.AddWithValue("@col_name", keyName) |> ignore
    cmd.Parameters.AddWithValue("@col_type", keyType) |> ignore
    cmd.Parameters.AddWithValue("@col_index", keyCol) |> ignore
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
        "insert into xlsx_cols(col_name, col_type, col_index) " + 
        "values (@col_name, @col_type, @col_index)"
    use cmd = new SQLiteCommand(cmdDDL, conn)
    cmd.Parameters.AddWithValue("@col_name", tableName) |> ignore
    cmd.Parameters.AddWithValue("@col_type", valType) |> ignore
    cmd.Parameters.AddWithValue("@col_index" ,colValNum) |> ignore
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    conn.Close()
    conn.Dispose()

let insertIntoTable (dbName : string) (tableName: string) (tag: string) (keyvalues: (string * string option)[] ) =
    
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()
    let trans = conn.BeginTransaction()

    keyvalues
    |> Array.iter (fun (key, maybeval) -> 
            let cmdDDL = 
                "insert into \"" + tableName + "\" (XlsxTag, XlsxKey, XlsxVal) " + 
                "values (@XlsxTag, @XlsxKey, @XlsxVal)"
            use cmd = new SQLiteCommand(cmdDDL, conn, trans)
            //cmd.Parameters.AddWithValue("@tableName", tableName) |> ignore
            cmd.Parameters.AddWithValue("@XlsxTag", tag) |> ignore
            cmd.Parameters.AddWithValue("@XlsxKey", key) |> ignore
            match maybeval with
            | None -> cmd.Parameters.AddWithValue("@XlsxVal" , DBNull.Value) |> ignore
            | Some colval -> cmd.Parameters.AddWithValue("@XlsxVal" , colval) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            cmd.Dispose()
        )
    
    trans.Commit()
    conn.Close()
    conn.Dispose()


let readCollValues (h : Header)   : Map<string, string option> = 
    // TO DO
    Map.empty
