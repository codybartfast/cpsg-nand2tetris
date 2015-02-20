import sys
import json

# Name of the Members
# Everything:
Token = 'Token'  # the token type

# Terminal:
Value = 'Value'
# Identifier:
Type = 'Type'
Kind = 'Kind'
Index = 'Index'
# SubroutineDec
VarCount = 'VarCount'
# SubroutineDec - constructor
FieldCount = 'FieldCount'

# NonTerminal
Children = 'Children'

def writeLine(line):
    print line
    
def readFileText():
    jsonFile = sys.argv[1]
    writeLine("Attempting to open file: " + jsonFile)
    fileText = open(jsonFile).read()
    writeLine("Read " + str(len(fileText)) + " characters.")
    return fileText


def codeSubroutineDec(subroutineDec):
    children = subroutineDec[Children]
    subroutineType = children[0][Value]
    type = children[1][Value]
    name = children[2][Value]
    localCount = subroutineDec[VarCount]

    if subroutineType == 'constructor':
        fieldCount = subroutineDec[FieldCount]
        suffix = ', oh and the class has ' + fieldCount + ' fields.'
    else:
        suffix = '.'
    line = '    ' + subroutineType.upper() + ' "' + name + '" returns a ' + type + \
        ', it has ' + localCount + ' local variables' + suffix
    writeLine(line)
    #  we don't need to do anything with the parameter list (used by symbol table)


def codeClass(_class):
    children = _class[Children]
    className = children[1][Value]
    writeLine('')
    writeLine("I'm in CLASS " + className)
    for child in children:
        if child[Token] == 'subroutineDec':
            codeSubroutineDec(child)
    #  N.B.
    #  We don't have to do anything with the classVarDecs.
    #  that information was used by the symbol table.
    writeLine("Ding, ding, end of CLASS " + className)
    writeLine('')


def main():
    fileText = readFileText()
    tree = json.loads(fileText)
    codeClass(tree)
  
main()
