// Cpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <algorithm>
#include <fstream>
#include <iostream>
#include <streambuf>
#include <string>
#include "../jsoncpp-master/include/json/json.h"

using namespace std;
using namespace Json;

static const string dv = "Defualt Value";

// Name of the Members
// Everything:
static const string sToken = "Token";

// Terminal:
static const string sValue = "Value";
// Identifier :
static const string sType = "Type";
static const string sKind = "Kind";
static const string sIndex = "Index";
// SubroutineDec
static const string sVarCount = "VarCount";
// SubroutineDec - constructor
static const string sFieldCount = "FieldCount";

// NonTerminal
static const string sChildren = "Children";


void writeLine(string line){
	cout << line << "\r\n";
	//cout << "\r\n";
}

string readFileText(string filePath){
	std::ifstream stream(filePath);
	std::string fileText((std::istreambuf_iterator<char>(stream)),
		std::istreambuf_iterator<char>());
	return fileText;
}

Value parseJson(string fileText){
	Json::Value tree;
	Json::Reader reader;
	reader.parse(fileText, tree);
	return tree;
}

void codeSubroutine(Value subroutineDec){
	Value children = subroutineDec.get(sChildren, dv);
	string subroutineType = children.get(0u, dv).get(sValue, dv).asString();
	string type = children.get(1, dv).get(sValue, dv).asString();
	string name = children.get(2, dv).get(sValue, dv).asString();
	string localCount = subroutineDec.get(sVarCount, dv).asString();
	
	string suffix;
	if (subroutineType == "constructor"){
		string fieldCount = subroutineDec.get(sFieldCount, dv).asString();
		suffix = ", oh and the class has " + fieldCount + " fields.";
	}
	else{
		suffix = ".";
	}
	string line = "    " + subroutineType + " '" + name + "' returns a " + type +
		", it has " + localCount + " local variables" + suffix;
	writeLine(line);
	//	we don't need to do anything with the parameter list (used by symbol table)
}

void codeClass(Value _class){
	Value children = _class.get(sChildren, dv);
	string className = children.get(1, dv).get(sValue, dv).asString();
	writeLine("");
	writeLine("I'm in CLASS " + className);

	int size = children.size();
	for (int i = 0; i < size; i++){
		Value child = children.get(i, dv);
		if (child.get(sToken, dv).asString() == "subroutineDec"){
			codeSubroutine(child);
		}
	}
	// N.B.
	//	We don't have to do anything the with classVarDecs.
	//	that information was used by the symbol table.
	writeLine("Ding, ding, end of CLASS " + className);
	writeLine("");
}

int _tmain(int argc, _TCHAR* argv[])
{
	string filePath = argv[1];
	string fileText = readFileText(filePath);

	Value tree = parseJson(fileText);
	codeClass(tree);

	writeLine("Press ENTER to continue.");
	cin.ignore();
	return 0;
}




