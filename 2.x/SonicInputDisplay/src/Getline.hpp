#ifndef GETLINE_H
#define GETLINE_H

#include <istream>
#include <string>
#include <vector>

//getline that works with any line endings
std::istream& getlineSafe(std::istream& is, std::string& t);

std::vector<std::string> readFileLines(const char* filename);

#endif
