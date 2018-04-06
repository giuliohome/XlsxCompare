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

let ofString (value:string) =
        if String.IsNullOrEmpty value then None else Some value

let cell2String (trasf: string -> string) (cell: IXLCell)=
    cell.GetString()
    |> ofString
    |> Option.map trasf
    |> StringCell

let dateTrasf = fun str ->
    (System.DateTime.Parse str).ToString("yyyy-MM-dd")
  
let intTrasf = fun str ->
    (System.Int32.Parse str).ToString("yyyy-MM-dd") 

let floatTrasf = fun str ->
    (System.Decimal.Parse str).ToString("yyyy-MM-dd") 

let readExcelCell (c: Header) (cell: IXLCell) : CellType =
    match c.colType with
    | StringCol -> 
        cell |> cell2String id
    | DateCol -> 
        cell |> cell2String dateTrasf
    | FloatCol ->
        cell |> cell2String floatTrasf
    | IntCol ->
        cell |> cell2String intTrasf

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


type LogChange = { keyValue: string; tagAfter: string; tagBefore: string option; fieldName: string; valueAfter: string option; valueBefore: string option}

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
                        companyRow.Field(c.Name)
                        |> readExcelCell c
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



let writeLogBook (xlsxPath : string) (changes : seq<LogChange>) =
    use wb = new XLWorkbook()
    use ws = wb.Worksheets.Add("Contacts")

    ws.Cell(1,1).Value <- "Contr. Key"
    ws.Cell(1,2).Value <- "Event Date"
    ws.Cell(1,3).Value <- "Previous Date"
    ws.Cell(1,4).Value <- "Field Name"
    ws.Cell(1,5).Value <- "Current Value"
    ws.Cell(1,6).Value <- "Previous Value"

    let rngHeaders = ws.Range("A1:F1")
    rngHeaders.Style.Alignment.Horizontal <- XLAlignmentHorizontalValues.Center
    rngHeaders.Style.Font.Bold <- true
    rngHeaders.Style.Font.FontColor <- XLColor.DarkBlue;
    rngHeaders.Style.Fill.BackgroundColor <- XLColor.Aqua;

    changes
    |> Seq.iteri ( fun i c -> 
        ws.Cell(2 + i,1).Value <- c.keyValue //"Contr. Key"
        ws.Cell(2 + i,2).Value <- c.tagAfter //"Event Date"
        match c.tagBefore with
        | Some tag -> ws.Cell(2 + i,3).Value <- tag //"Previous Date"
        | _ -> ()
        ws.Cell(2 + i,4).Value <- c.fieldName //"Field Name"
        match c.valueAfter with
        | Some avalue -> ws.Cell(2 + i,5).Value <- avalue //"Current Value"
        | _ -> ()
        match c.valueBefore with
        | Some avalue -> ws.Cell(2 + i,6).Value <- avalue //"Previous Value"
        | _ -> ()
    )

    ws.Columns().AdjustToContents() |> ignore 
    ws.SheetView.Freeze(1,1)
    wb.SaveAs(xlsxPath)