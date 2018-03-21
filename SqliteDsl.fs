module SqliteDsl
open XlsxDsl

let firstImport2Sqlite<'a> 
        (xlsxTag: 'a)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =
        //TO-DO
        let colKey = xlsxCols.[keyColNum]
        printfn "the columns %A (type %s) and %s (type %s) \nare the keys for the following tables" 
            xlsxTag (xlsxTag.GetType().ToString())
            colKey.header.Name (colKey.header.colType.ToString())

        [| 0 .. (xlsxCols.Length - 1)|]
        |> Array.except [|keyColNum|]
        |> Array.iter( fun i ->
                printfn "creating %s sqlite table with values of type %s" 
                    xlsxCols.[i].header.Name (xlsxCols.[i].header.colType.ToString())
                let numValues = xlsxCols.[i].Cells.Length
                printfn "there are %d values to be inserted: from %A to %A" 
                    numValues xlsxCols.[i].Cells.[0] xlsxCols.[i].Cells.[numValues-1]
            )
            
        "First Excel Imported into new Sqlite DB"
