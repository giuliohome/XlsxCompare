# XlsxCompare

A program libary, which stores records from many excel files and then, based on a key column, produces a log of the differences

Nothing special, just a thin wrapper layer around closedxml and some sqlite to achieve the above said, simple goal. 
The idea is to describe the xlsx header with a list of generic DU types, with the name and the format (string, date, float, int). 
I use a generic a' (e.g. date time event), provided a function to show a' to string, to distinguish the different xlsx files. 
The only thing I still have to do is to import them all into sqlite - under these generic columns definition - 
and to produce the differences, based on a certain input key... 