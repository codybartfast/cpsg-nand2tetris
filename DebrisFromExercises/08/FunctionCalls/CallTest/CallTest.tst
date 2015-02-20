// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/08/FunctionCalls/SimpleFunction/SimpleFunction.tst

load CallTest.asm,
output-file CallTest.out,
compare-to CallTest.cmp,
output-list RAM[0]%D1.6.1  RAM[20]%D1.6.1;
            
set RAM[0] 20,
set RAM[1] 701,
set RAM[2] 702,
set RAM[3] 703,
set RAM[4] 704,
     
repeat 200 {
  ticktock;
}

output;
