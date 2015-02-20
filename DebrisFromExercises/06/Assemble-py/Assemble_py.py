import os
import re
import sys

symbolTable = {
    "SP": 0,
    "LCL": 1,
    "ARG": 2,
    "THIS": 3,
    "THAT": 4,
    "SCREEN": 16384,
    "KBD": 24576,
    "R0": 0,
    "R1": 1,
    "R2": 2,
    "R3": 3,
    "R4": 4,
    "R5": 5,
    "R6": 6,
    "R7": 7,
    "R8": 8,
    "R9": 9,
    "R10": 10,
    "R11": 11,
    "R12": 12,
    "R13": 13,
    "R14": 14,
    "R15": 15,           
}

computes = {
    "0":"101010",
    "1":"111111",
    "-1":"111010",
    "D":"001100",
    "A":"110000",
    "!D":"001101",
    "!A":"110001",
    "-D":"001111",
    "-A":"110011",
    "D+1":"011111",
    "A+1":"110111",
    "D-1":"001110",
    "A-1":"110010",
    "D+A":"000010",
    "D-A":"010011",
    "A-D":"000111",
    "D&A":"000000",
    "D|A":"010101"
}

jumps = {
    "null":"000",
    "JGT":"001",
    "JEQ":"010",
    "JGE":"011",
    "JLT":"100",
    "JNE":"101",
    "JLE":"110",
    "JMP":"111",
}


asmPath=sys.argv[1]
hackPath = re.sub("\.asm$", ".hack", sys.argv[1])
hackFile = open(hackPath, 'w')

commands = map(str.strip, open(asmPath).readlines())
commands = [command for command in commands if re.match("^(?!/)\S", command)]

references = set()
nextInstruction = 0

for command in commands:
    if command[0] == "(":
        symbolTable[command[1:-1]] = nextInstruction
        continue
    
    if re.match("^@\D", command):
        references.add(command[1:])
    nextInstruction += 1

nextVariable = 16
variables = references - set(symbolTable)
for variable in variables:
    symbolTable[variable] = (nextVariable)
    nextVariable += 1

for command in commands:
    if command[0] == "(":
        continue

    if command[0] == "@":
        address = int(command[1:]) if re.match("^@\d", command) else symbolTable[command[1:]]
        hackFile.write("{0:b}".format(address).zfill(16) + '\n')
        continue

    dest = ""
    jump = "null"
 
   
    if "=" in command:
        dest, compJump = command.split("=")
    else:
        dest = "000"
        compJump = command

    if ";" in compJump:
        comp, jump = compJump.split(";")
    else:
        jump="null"
        comp=compJump    

    instruction = "111" + \
        ("1" if "M" in comp else "0") + \
        computes[comp.replace("M", "A")]  + \
        ("1" if "A" in dest else "0") + \
        ("1" if "D" in dest else "0") + \
        ("1" if "M" in dest else "0") + \
        jumps[jump]
    
    instruction.zfill(16)

    hackFile.write(instruction + '\n')

hackFile.close()

