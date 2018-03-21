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