#ifndef SPLIT_H
#define SPLIT_H

#include <vector>
#include <string>

/* Parse a line and return an array of the
individual tokens. Original line is modified. 
Make sure to call free() on the returned char** */
char** split(char* line, char delim, int* length);

//Splits a string into individual tokens based on 
// a delimiter character. Original string is modified.
// Does not use the heap, so no need to free() anything.
// Instead, you pass in a buffer of fixed size to
// where the pointers will be placed. You pass in
// the length of the tokenPointers array as the last
// parameter.
void split(char* line, char delim, int* numFound, char** tokenPointers, int maxNumTokensToFind);

//given a string, split the string into a vector of strings
// based on some delimiter character. the string canont be more 
// than 1023 characters long.
std::vector<std::string> split(std::string line, char delim);

#endif
