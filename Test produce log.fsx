#r @"packages\ClosedXML.0.91.0\lib\net452\ClosedXML.dll"
#r @"packages\DocumentFormat.OpenXml.2.7.2\lib\net46\DocumentFormat.OpenXml.dll"
#r @"packages\FastMember.Signed.1.1.0\lib\net40\FastMember.Signed.dll"
#r @"packages\System.Data.SQLite.Core.1.0.108.0\lib\net46\System.Data.SQLite.dll"
#r @"packages\ExcelNumberFormat.1.0.3\lib\net20\ExcelNumberFormat.dll"
#load "utils.fs"
#load "XlsxDsl.fs"
#load "SqliteInterpreter.fs"
#load "SqliteDsl.fs"
#load "XlsxCompare.fs"

open XlComp
open XlsxDsl
open System


let xlsxSchema = 
    [|
        {colType = DateCol; Name = "Registration Date"}; 
        {colType = StringCol; Name = "ContrKey"};
        {colType = StringCol; Name =  "Cpty"};
        {colType = StringCol; Name =  "Curr"};
        {colType = StringCol; Name =  "UoM"};
        {colType = FloatCol; Name =  "Price"};
        {colType = DateCol; Name =  "Request Date"};
        {colType = StringCol; Name =  "Broker"};
        {colType = DateCol; Name =  "Start Date"};
        {colType = DateCol; Name =  "End Date"};
        {colType = FloatCol; Name =  "Qty"};
        {colType = StringCol; Name =  "Qty UoM"};
        {colType = IntCol; Name =  "TC Order"};
        {colType = DateCol; Name =  "CharterParty Date"};
        {colType = StringCol; Name =  "CharterParty Full Name"};
        {colType = StringCol; Name =  "Trader"};
        {colType = FloatCol; Name =  "Market Value"};
    |] 

let fields = xlsxSchema |> Array.map (fun s -> s.Name) |> Array.except([|"ContrKey"|])

// Let's use library scripting code for testing purposes

let comp = XlsxCompare()

// in-memory, just for testing purposes
["contr01"; "contr27"; "contr39"; "contr96"]
|> List.iter (fun key-> 
    let changes = comp.log2Mem @"C:\test\xlsxCompare\logbook_test.s3db" fields key 
    printfn "produced %d changes for field %s" (changes |> Seq.length) key
    changes
    |> Seq.iter (fun c ->
           printfn "%A" c ) )

// second option, writing the LogBook to Excel, e.g.
comp.log2Excel 
    @"C:\test\xlsxCompare\logbook_contr96.xlsx"
    @"C:\test\xlsxCompare\logbook_test.s3db" fields "contr96" 