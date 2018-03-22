module SqliteDsl
open XlsxDsl

let firstImport2Sqlite<'a> 
        (xlsxTag: 'a)
        (showTag: 'a -> string)
        (keyColNum : int)
        (sqlitePath : string) 
        (xlsxCols : ColValues[]) 
        =
        let colKey = xlsxCols.[keyColNum]
        printfn "\nthe columns XlsxKey (%A of type %A) and %s (type %s) \nare the keys for the following tables" 
            (showTag(xlsxTag)) (xlsxTag.GetType())
            colKey.header.Name (colKey.header.colType.ToString())

        [| 0 .. (xlsxCols.Length - 1)|]
        |> Array.except [|keyColNum|]
        |> Array.iter( fun i ->
                let header = xlsxCols.[i].header
                printfn "creating %s sqlite table with values of type %A" 
                    header.Name header.colType
                let cells = xlsxCols.[i].Cells
                let numValues = cells.Length
                printfn "there are %d values to be inserted: from %A to %A"  
                    numValues 
                    cells.[0] 
                    cells.[numValues-1]
            )

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
