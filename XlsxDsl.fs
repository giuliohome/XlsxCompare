module XlsxDsl
open System
open ClosedXML.Excel

type CellType =
    | DateCell of DateVal : DateTime option
    | StringCell of StringVal : string option
    | FloatCell of FloatVal : float option
    | IntCell of IntVal : int option


type ColType =
    | DateCol 
    | StringCol 
    | FloatCol 
    | IntCol 

type Header = { colType : ColType; Name : string}
type ColValues =  { header : Header ; Cells : CellType[]}

let readCell2String (cell : CellType) = 
    match cell with
    | StringCell maybeText -> maybeText
    | DateCell maybeDate -> 
        match maybeDate with
        | None -> None
        | Some date -> date.ToString("yyyy-MM-dd") |> Some
    | IntCell maybeInt -> 
        match maybeInt with
        | None -> None
        | Some i -> i.ToString() |> Some
    | FloatCell maybeFloat -> 
        match maybeFloat with
        | None -> None
        | Some f -> f.ToString() |> Some


let readXlsx         
        (xlsxPath : string) 
        (sheetName: string)
        (colSchema: Header[])
    =
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

        xlsxCols