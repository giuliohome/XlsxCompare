module SqliteDsl
open XlsxDsl

let firstImport2Sqlite<'a> 
        (xlsxTag: 'a)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =
        //TO-DO
        "First Excel Imported into new Sqlite DB"
