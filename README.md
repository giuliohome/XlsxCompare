# XlsxCompare

A program libary, which stores records from many excel files and then, based on a key column, produces a log of the differences

Nothing special, just a thin wrapper layer around closedxml and some sqlite to achieve the above said, simple goal. 
The idea is to describe the xlsx header with a list of generic DU types, with the name and the format (string, date, float, int). 
I use a generic a' (e.g. date time event), provided a function to show a' to string, to distinguish the different xlsx files. 
The only thing I still have to do is to import them all into sqlite - under these generic columns definition - 
and to produce the differences, based on a certain input key... 

# Example

Let's say that you have 3 excel files and their ordering parameter is a date 
and we'll choose the column `B` (`Code`) as the key to track changes.
So the first will be imported as `2018-01-20`.

![img1](imgages/example01.jpg)

The second as `2018-02-07`

![img2](imgages/example02.jpg)

And the third as `2018-03-01`

![img3](imgages/example03.jpg)

Now, we want to produce the changes log for `Code2` in the following log-book

![img3](imgages/logbook.jpg)

Imagine hundreds of excel files with hundreds of rows and dozens of columns and you'll guess the reason why a tool is needed.


