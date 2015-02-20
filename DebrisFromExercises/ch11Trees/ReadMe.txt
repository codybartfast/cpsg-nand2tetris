XML Files
=========
These directories contain the compiler output for each of 
the chapter 10 and chapter 11 projects.

E.g. some of the files for the \10\Square\  project are...
		
Square.jack   - Source file
SquareT.xml   - Flat token list 
Square.xml    - Syntax tree
SquareWSI.xml - "With Symbol Information" identifiers have 
                extra attributes with the information from
                the Symbol table.

The chapter 10 directories also contain an "Expected" 
directory which contains the provided token and syntax 
files for comparison.

The "OSPlus" directory contains the compiled VM code (plus
the OS files) which can be loaded into the VMEmulator.


Compiler
========
The compile can be run with:
		JackCompiler  directoryPath
or
		JackCompiler  filePath

Source code for the Compiler:
https://github.com/it-depends/FSS/tree/master/Nand2Tetris/10/JackCompiler