﻿#r @"packages\ClosedXML.0.91.0\lib\net452\ClosedXML.dll"
#r @"packages\DocumentFormat.OpenXml.2.7.2\lib\net46\DocumentFormat.OpenXml.dll"
#r @"packages\FastMember.Signed.1.1.0\lib\net40\FastMember.Signed.dll"
#r @"packages\System.Data.SQLite.Core.1.0.108.0\lib\net46\System.Data.SQLite.dll"
#load "utils.fs"
#load "XlsxDsl.fs"
#load "SqliteInterpreter.fs"
#load "SqliteDsl.fs"
#load "XlsxCompare.fs"

open XlComp
open XlsxDsl
open System


// Let's use library scripting code for testing purposes

let comp = XlsxCompare()

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

comp.initFirstXlsx @"C:\test\xlsxCompare\file_01.xlsx" "My Sheet Name" 
    xlsxSchema
    (DateTime(2018,3,10)) // xlsx key parameter for import 
    // a generic type, provided a function to show a' to string, 
    // in this test-case an event date                      
    (fun d -> d.ToString("yyyy-MM-dd")) // for ordering reasons, it'll be used in the events log track
    1 // the index is zero-based: "ContrKey" is the key for diff-ing
    @"C:\test\xlsxCompare\logbook_test.s3db"
|> printfn "\n%s"

comp.nextXlsx @"C:\test\xlsxCompare\file_02.xlsx" "My Sheet Name" 
    xlsxSchema
    (DateTime(2018,4,1)) // must be in ascending order: later than the previous import                     
    (fun d -> d.ToString("yyyy-MM-dd")) // must be the same as the previous imports
    1 // must be the same as the previous imports
    @"C:\test\xlsxCompare\logbook_test.s3db"
|> printfn "\n%s"
