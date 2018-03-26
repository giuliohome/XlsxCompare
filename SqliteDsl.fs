module SqliteDsl
open XlsxDsl
open SqliteInterpreter
open utils // concatenate maps


let reduceKeyValue (keyColNum : int) (valColNum : int) (beforeMap : Map<string, string option>) (xlsxColsNew : ColValues[]) : Map<string, string option> =
    let beforeKeys = 
        beforeMap 
        |> Map.toArray
        |> Array.map (fun (k,v) -> k)
        |> Set.ofArray
    let currentKeys = 
        xlsxColsNew.[keyColNum].Cells
        |> Array.choose readCell2String
        |> Set.ofArray
    let currentMap = 
        xlsxColsNew.[keyColNum].Cells
        |> Array.mapi ( fun i ct ->
            match readCell2String ct with
            | None -> None
            | Some key -> Some (key, readCell2String xlsxColsNew.[valColNum].Cells.[i])
            )
        |> Array.choose id
        |> Map.ofArray
    let deletedKeys = beforeKeys - currentKeys
    let insertedKeys = currentKeys - beforeKeys
    let possiblyUpdatedKeys = Set.intersect beforeKeys currentKeys
    let reallyUpdatedKeys = 
        possiblyUpdatedKeys
        |> Set.toArray
        |> Array.where ( fun upd ->
            currentMap.Item(upd) = beforeMap.Item(upd) |> not
        )
    let deletedKeysMap : Map<string, string option> = 
        deletedKeys
        |> Set.toArray
        |> Array.map(fun k -> (k,None))
        |> Map.ofArray
    let insertedAndUpdatedKeysMap = 
        insertedKeys
        |> Set.toArray
        |> Array.append reallyUpdatedKeys
        |> Array.map(fun k -> (k, currentMap.Item(k)))
        |> Map.ofArray

            
    deletedKeysMap
    |> merge (fun k (v1,v2) -> Some "error") insertedAndUpdatedKeysMap // keys shouldn't overlap
          

let importDDL2Sqlite<'a> 
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =

        let colKey = xlsxCols.[keyColNum]
        createLogBook sqlitePath colKey.header.Name (colKey.header.colType.ToString().Replace("Col","")) keyColNum
        printfn "\nthe columns XlsxKey (%A of type %A) and %s (type %A) \nare the keys for the following tables" 
            (showTag(xlsxTag)) (xlsxTag.GetType())
            colKey.header.Name (colKey.header.colType)

        [| 0 .. (xlsxCols.Length - 1)|]
        |> Array.except [|keyColNum|]
        |> Array.iter( fun i ->
                let header = xlsxCols.[i].header
                printfn "creating %s sqlite table with values of type %A" 
                    header.Name header.colType
                createTable sqlitePath header.Name i (colKey.header.colType.ToString().Replace("Col","")) (header.colType.ToString().Replace("Col",""))
            )


let importDML2Sqlite<'a> 
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =
        //TO DO - Optimization of the Gaps and Islands Pattern is:
        //Don't Repeat Yourself
        //We have to insert only the effective changes, not all the keys and values
        [| 0 .. (xlsxCols.Length - 1)|]
        |> Array.except [|keyColNum|]
        |> Array.iter( fun i ->
                let cells = xlsxCols.[i].Cells
                let numValues = cells.Length 
                // we have group all the key and select
                // the val before of the max tag < xlsxTag
                // the val after of the min tag > xlstag
                // | val before is the same -> skip current value
                // | val after is the same -> insert current value and delete the value after
                // | _ -> insert current value
                // TO MAKE IT SIMPLE => we impose that the tagged xlsx files are imported in stricly ascending order of tag
                // all this logic will be managed inside the function reduceKeyValue
                let xlsxColBefore = readCollValues xlsxCols.[i].header
                let optimizedInserts = reduceKeyValue keyColNum i xlsxColBefore xlsxCols |> Map.toArray 
                printfn "there are %d values to be inserted into table %s: from %A to %A out %d (duplicated) from  from %A to %A"  
                    optimizedInserts.Length
                    xlsxCols.[i].header.Name
                    (if optimizedInserts.Length >0 then optimizedInserts.[0] else "-" , Some "-" )
                    (if optimizedInserts.Length >0 then Array.last optimizedInserts else "-" , Some "-" )
                    numValues 
                    cells.[0] 
                    cells.[numValues-1]
                insertIntoTable sqlitePath xlsxCols.[i].header.Name  (showTag xlsxTag) optimizedInserts
            )    

let firstImport2Sqlite<'a> 
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =
        importDDL2Sqlite xlsxTag showTag keyColNum sqlitePath xlsxCols
        importDML2Sqlite xlsxTag showTag keyColNum sqlitePath xlsxCols

        //then I can track the log changes by
        //[wrong] grouping (= distinct values = changes) and taking first, last value 
        //to rebuild the value before and after in the fsharp logic 
        //this falls under the Gaps and Islands Pattern, where a traditional MIN/MAX/COUNT/GROUP BY won’t suffice.
        //I have a table (key will be date instead of int, but we can abstract this out, we just need ordering)
        //key | val
        //1   |  v1
        //3   |  v1
        //7   |  v2
        //12  |  v2
        //15  |  v2
        //24  |  v1
        //I want a change log.  I guess I group by val with first and last key. But I need
        //v1 from 1 to 3
        //v2 from 7 to 15
        //v1 from 24 to 24 <== that's the issue
        //Here is my 🆒 solution on #sqlite http://www.sqlfiddle.com/#!5/ace80/1 
        //  select 
        //  null as 'val before',null as 'key before', val as 'val after', key as 'key after'
        //  from Table1 where key = (select min(key) from Table1)
        //  union
        //  select
        //  before.val as 'val before', before.key as 'key before', after.val as 'val after', after.key as 'key after'
        //  from Table1 before left join Table1 after on after.val <> before.val and after.key = 
        //  (select key from Table1 where key > before.key
        //   order by key asc limit 1) where after.key is not null
        //  order by "key before"

        //TO-DO
            
        "First Excel Imported into new Sqlite DB"


    