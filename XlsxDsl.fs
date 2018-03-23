module XlsxDsl
open System

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

        