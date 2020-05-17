# DBFReaderFinal
Features:
1. Read local DBF files for number of days from the specified directory. 
2. Store data in SQL lite db. 
3. If the data is stored for the day, the same will not be stored again. 
4. You do not need to create tables, the application will create table based on the specified files in the sttings.

Dependencies:
1. Need to install Foxpro driver. You can download the driver from https://www.microsoft.com/en-in/download/details.aspx?id=14839
2. Need visual C# (preferably visual studio 2017 - any edition will work)

How to run the file:
1. Download the code and open the solution (.sln) in visual studio. Check setting file in solution explorer.
2. Check if the specified folder exists on not (folder to read as YYYYMMDD pattern)

That's it! Enjoy
