#include <istream>
#include <string>
#include <fstream>
#include <vector>
#include "Getline.hpp"

std::istream& getlineSafe(std::istream& is, std::string& t)
{
    std::string myline;

    if (getline(is, myline))
    {
       if (myline.size() && myline[myline.size()-1] == '\r')
       {
           t = myline.substr(0, myline.size() - 1);
       }
       else
       {
           t = myline;
       }
    }
    else
    {
        t.clear();
    }

    return is;
}

std::vector<std::string> readFileLines(const char* filename)
{
    std::vector<std::string> lines;

    std::ifstream file(filename);
    if (!file.is_open())
    {
        std::fprintf(stdout, "Error: Cannot load file '%s'\n", filename);
        file.close();
        return lines;
    }

    while (!file.eof())
    {
        std::string s;
        getlineSafe(file, s);
        lines.push_back(s);
    }

    file.close();

    return lines;
}
