#include <stdlib.h>
#include <vector>
#include <string>
#include <cstring>

#include "Split.hpp"

char** split(char* line, char delim, int* length)
{
    /* Scan through line to find the number of tokens */
    int numTokens = 0;
    int index = 0;
    int inToken = 0;

    while (line[index] != 0)
    {
        if (line[index] != delim && inToken == 0)
        {
            inToken = 1;
            numTokens += 1;
        }
        else if (line[index] == delim)
        {
            inToken = 0;
        }
        index += 1;
    }

    /* Get memory to store the data */
    char ** parsedData = (char**)malloc(sizeof(char*)*(numTokens + 1));

    /* Scan through line to fill parsedData
    and set 0 characters after tokens*/
    index = 0;
    inToken = 0;
    int tokenNum = 0;

    while (line[index] != 0)
    {
        if (line[index] != delim && inToken == 0)
        {
            parsedData[tokenNum] = &line[index];
            tokenNum += 1;
            inToken = 1;
        }
        else if (line[index] == delim)
        {
            if (inToken == 1)
            {
                line[index] = 0;
            }
            inToken = 0;
        }
        index += 1;
    }

    parsedData[numTokens] = NULL;

    *length = numTokens;

    return parsedData;
}

void split(char* line, char delim, int* numFound, char** tokenPointers, int maxNumTokensToFind)
{
    int numTokensFound = 0;
    int index = 0;
    bool inToken = false;

    while (line[index] != 0 && numTokensFound < maxNumTokensToFind)
    {
        if (line[index] != delim && !inToken)
        {
            inToken = true;
            line[index] = 0;
            tokenPointers[numTokensFound] = &line[index];
            numTokensFound += 1;
        }
        else if (line[index] == delim)
        {
            inToken = false;
        }
        index += 1;
    }

    *numFound = numTokensFound;
}

//given a string, split the string into a vector of strings
// based on some delimiter character. the string canont be more
// than 1023 characters long.
std::vector<std::string> split(std::string line, char delim)
{
    if (delim == 0)
    {
        std::vector<std::string> list;
        std::fprintf(stderr, "Cannot split a string on null character.\n");
        return list;
    }

    if (line.size() >= 1023)
    {
        std::vector<std::string> list;
        std::fprintf(stderr, "string sent to split() is too long.\n");
        return list;
    }

    char lineBuf[1024];
    memset(lineBuf, 0, 1024);
    memcpy(lineBuf, line.c_str(), line.size());

    // Scan through line to find the number of tokens
    int numTokens = 0;
    int index = 0;
    int inToken = 0;

    while (line[index] != 0)
    {
        if (line[index] != delim && inToken == 0)
        {
            inToken = 1;
            numTokens += 1;
        }
        else if (line[index] == delim)
        {
            inToken = 0;
        }
        index += 1;
    }

    // Get memory to store the data
    char** parsedData = (char**)malloc(sizeof(char*)*(numTokens + 1));

    // Scan through line to fill parsedData
    //  and set null characters after tokens
    index = 0;
    inToken = 0;
    int tokenNum = 0;

    while (line[index] != 0)
    {
        if (line[index] != delim && inToken == 0)
        {
            parsedData[tokenNum] = &line[index];
            tokenNum += 1;
            inToken = 1;
        }
        else if (line[index] == delim)
        {
            if (inToken == 1)
            {
                line[index] = 0;
            }
            inToken = 0;
        }
        index += 1;
    }

    parsedData[numTokens] = nullptr;

    std::vector<std::string> dat;

    for (int i = 0; i < numTokens; i++)
    {
        dat.push_back(parsedData[i]);
    }

    free(parsedData);

    return dat;
}
