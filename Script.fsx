#r @"C:\shipping\2018xlsm\xlsx2sqlite\XlComp\packages\ClosedXML.0.91.0\lib\net452\ClosedXML.dll"
#r @"C:\shipping\2018xlsm\xlsx2sqlite\XlComp\packages\DocumentFormat.OpenXml.2.7.2\lib\net46\DocumentFormat.OpenXml.dll"
#r @"C:\shipping\2018xlsm\xlsx2sqlite\XlComp\packages\FastMember.Signed.1.1.0\lib\net40\FastMember.Signed.dll"
#load "XlsxDsl.fs"
#load "SqliteDsl.fs"
#load "XlsxCompare.fs"

open XlComp
open XlsxDsl
open System


// Let's use library scripting code for testing purposes

let comp = XlsxCompare()
comp.initFirstXlsx @"C:\my_path\test_it.xlsx" "My Sheet Name" 
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
    DateTime.Now.Date 1 ""
|> printfn "\n%s"
