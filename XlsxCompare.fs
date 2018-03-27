namespace XlComp
open System
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

        let xlsxCols = readXlsx xlsxPath sheetName colSchema
        
        // now we need to import our xlsxCols into Sqlite
        firstImport2Sqlite xlsxTag showTag keyColNum sqlitePath xlsxPath xlsxCols
        
    member this.nextXlsx<'a> 
        (xlsxPath : string) 
        (sheetName: string)
        (colSchema: Header[])
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) = 

        let xlsxCols = readXlsx xlsxPath sheetName colSchema
        
        // now we need to import our xlsxCols into Sqlite
        nextImport2Sqlite xlsxTag showTag keyColNum sqlitePath xlsxPath xlsxCols


    member this.log2Mem (sqlitePath : string) (fields : string[]) (keyVal : string) = 
        produceLog  sqlitePath fields keyVal

    member this.log2Excel (xlsxPath : string) (sqlitePath : string) (fields : string[]) (keyVal : string) = 
        produceLog sqlitePath fields keyVal
        |> writeLogBook xlsxPath