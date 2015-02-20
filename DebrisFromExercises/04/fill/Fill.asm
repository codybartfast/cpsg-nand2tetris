// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input. 
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel. When no key is pressed, the
// program clears the screen, i.e. writes "white" in every pixel.

// Put your code here.


@SCREEN
D=A
@Start  	// Holds start address of area to flll
M=D

// @4
@8192
D=A
@Size     // Number of address in area
M=D

@Start
D=M
@Size			
D=D+M
@End      // Address After last address in Area
M=D

(CheckKeyboard)
// set default value (white)
@Value
M=0

// read keyboard
@KBD
D=M

// Jump to Redraw if no key is pressed
@Redraw
D;JEQ

// set value to 1's (black)
@Value
M=-1

(Redraw)
// set cell address to start
@Start
D=M
@Cell
M=D

(DrawCell)
// Check cell is within bounds

// Find how far Cell is below End
@Cell
D=M
@End
D=M-D

// If not positive go back CheckKeyboard
@CheckKeyboard
D;JLE

// Actual do some srawing

// load value 
@Value
D=M

// Load current address
@Cell
A=M

// Set cell value
M=D

// Increment Cell
@Cell
M=M+1

// Draw next Cell
@DrawCell
0;JMP
