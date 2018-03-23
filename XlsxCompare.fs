namespace XlComp
open System
open ClosedXML.Excel
open DocumentFormat.OpenXml.Spreadsheet
open XlsxDsl
open SqliteDsl

// stores multiple xlsx into a sqlite db and track changes based on a key column
type XlsxCompare() = 
// the first xslx determines the sqlite initialization
// we use a type 'a (e.g. event date as DateTime or a simple description as string) to map each file
    member this.initFirstXlsx<'a> 
        (xlsxPath : string) 
        (sheetName: string)
        (colSchema: Header[])
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) = 

        use wb = new XLWorkbook(xlsxPath)
        use ws = wb.Worksheet(sheetName)
        let firstRowUsed = ws.FirstRowUsed()
        let categoryRow = firstRowUsed.RowUsed()
        let firstPossibleAddress = ws.Row(categoryRow.RowNumber()).FirstCell().Address
        let lastPossibleAddress = ws.LastCellUsed().Address
        let xlsxRange = ws.Range(firstPossibleAddress, lastPossibleAddress).RangeUsed()
        let xlsxTable = xlsxRange.AsTable()

        let xlsxCols =
            colSchema
            |> Array.map (fun c -> 
                {
                header = c; 
                Cells = 
                    xlsxTable.DataRange.Rows( fun (r: IXLTableRow) -> true )
                    |> Seq.map (fun (companyRow : IXLTableRow) ->  
                        companyRow.Field(c.Name).GetString()
                        |> (fun s -> 
                            match s with
                            | null 
                            | ""    -> StringCell None
                            | str   -> StringCell (Some str))
                    ) |> Seq.toArray
                })

        let colNum = xlsxCols.Length - 1
        let rowNum = xlsxCols.[colNum].Cells.Length - 1 
        printfn "table col %d  row %d " (colNum+1) (rowNum+1)

        printfn "last row"
        [|0..colNum|]
        |> Array.iter (fun i -> 
            match xlsxCols.[i].Cells.[rowNum] with
            | StringCell str -> printf "%s %A" "" str
            | _ -> printf "N/A"
        )
        
        // now we need to import our xlsxCols into Sqlite
        firstImport2Sqlite xlsxTag showTag keyColNum sqlitePath xlsxCols
        
