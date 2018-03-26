module SqliteInterpreter
open XlsxDsl
open System.Data.SQLite
open System

let createLogBook (dbName : string) (keyName : string) (keyType : string)  (keyCol : int)= 
    SQLiteConnection.CreateFile(dbName)
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()

    let cmdSql = 
        "create table xlsx_cols (" +
        "col_name VARCHAR(15), " +
        "col_type VARCHAR(10), " +
        "col_index INT, " + 
        "PRIMARY KEY(col_name) )"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdSql = 
        "create table xlsx_key (" +
        "col_name VARCHAR(15), " +
        "col_type VARCHAR(10), " +
        "col_index INT, " + 
        "PRIMARY KEY(col_name) )"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdSql = 
        "insert into xlsx_key(col_name, col_type, col_index) " + 
        "values (@col_name, @col_type, @col_index)"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.Parameters.AddWithValue("@col_name", keyName) |> ignore
    cmd.Parameters.AddWithValue("@col_type", keyType) |> ignore
    cmd.Parameters.AddWithValue("@col_index", keyCol) |> ignore
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()

    let cmdSql = 
        "create table xlsx_imports (" +
        "ImportedOn DATE, " +
        "XlsxTag VARCHAR(15), " +
        "XlsxPath VARCHAR(50), " + 
        "PRIMARY KEY(XlsxTag) )"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    conn.Close()
    conn.Dispose()


let tagXlsxPath (dbName : string) (xlsxPath : string) (xlsxTag : string) =
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()

    let cmdSql = 
        "insert into xlsx_imports(ImportedOn, XlsxTag, XlsxPath) " + 
        "values (@ImportedOn, @XlsxTag, @XlsxPath)"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.Parameters.AddWithValue("@ImportedOn", DateTime.Now) |> ignore
    cmd.Parameters.AddWithValue("@XlsxTag", xlsxTag) |> ignore
    cmd.Parameters.AddWithValue("@XlsxPath" ,xlsxPath) |> ignore
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    conn.Close()
    conn.Dispose()

let createTable (dbName : string) (tableName: string) (colValNum: int) (keyType: string) (valType: string) =
    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()
    
    let cmdSql = 
        "create table \"" + tableName + "\" (" +
        "XlsxTag VARCHAR(15), " +
        "XlsxKey TEXT(15) , " + 
        "XlsxVal TEXT(50) , " + 
        "PRIMARY KEY(XlsxTag,XlsxKey) )"
    use cmd = new SQLiteCommand(cmdSql, conn)
    cmd.ExecuteNonQuery() |> ignore
    cmd.Dispose()
    
    let cmdSql = 
        "insert into xlsx_cols(col_name, col_type, col_index) " + 
        "values (@col_name, @col_type, @col_index)"
    use cmd = new SQLiteCommand(cmdSql, conn)
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
            let cmdSql = 
                "insert into \"" + tableName + "\" (XlsxTag, XlsxKey, XlsxVal) " + 
                "values (@XlsxTag, @XlsxKey, @XlsxVal)"
            use cmd = new SQLiteCommand(cmdSql, conn, trans)
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
    
let readCollValues (dbName : string) (h : Header) : Map<string, string option> = 
    // TO DO
    let cmdSql = @"select  GKey.key, GKey.tag, T.XlsxVal from 
                    (select XlsxKey as 'key', 
                        max(XlsxTag) as 'tag'
                        from ""TableName""
                        group by XlsxKey
                        order by XlsxKey asc) as GKey
                    join ""TableName"" as T  on 
                        T.XlsxKey = GKey.key and T.XlsxTag = GKey.tag".Replace("TableName",h.Name)

    let connStr = sprintf "Data Source=%s;Version=3;" dbName
    use conn = new SQLiteConnection(connStr)
    conn.Open()

    use cmd = new SQLiteCommand(cmdSql, conn)
    let DR = cmd.ExecuteReader()
    let records_as_list = 
        [
            while DR.Read() do
                yield 
                    (
                    DR.["key"] :?> string , 
                    if DR.IsDBNull(2) 
                        then 
                            None 
                        else 
                            DR.GetString(2)
                            |> Some
                    )
        ]
    cmd.Dispose()

    conn.Close()
    conn.Dispose()
    records_as_list |> Map.ofList



