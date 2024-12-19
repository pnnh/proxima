#include "String.h"

#include <algorithm>
#include <sstream>
#include <utility>

template <typename... Args>
std::string quark::PSString::DynamicPrint(std::string_view rt_fmt_str, Args&&... args)
{
    return std::vformat(rt_fmt_str, std::make_format_args(args...));
}

bool quark::PSString::IsBlank(const std::string& str)
{
    return str.empty() || std::all_of(str.begin(), str.end(), [](unsigned char c) { return std::isspace(c); });
}

bool quark::PSString::StartsWith(const std::string& str, const std::string& prefix)
{
    return str.size() >= prefix.size() && str.compare(0, prefix.size(), prefix) == 0;
}

std::string quark::PSString::LeftReplace(const std::string& str, const std::string& prefix,
                                                 const std::string& newPrefix)
{
    if (StartsWith(str, prefix))
    {
        return newPrefix + str.substr(prefix.size());
    }
    return str;
}

bool quark::PSString::EndsWith(const std::string& str, const std::string& suffix)
{
    return str.size() >= suffix.size() && str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
}

std::vector<std::string> quark::PSString::Split(const std::string& str, char delimiter)
{
    std::vector<std::string> stringList;
    std::istringstream iss(str);
    std::string s;
    while (getline(iss, s, delimiter))
    {
        stringList.push_back(s);
    }
    return stringList;
}

std::string quark::PSString::toLower(const std::string& str)
{
    auto newStr = std::string(str);
    std::ranges::transform(newStr, newStr.begin(), ::tolower);
    return newStr;
}

std::string quark::PSString::toUpper(const std::string& str)
{
    auto newStr = std::string(str);
    std::ranges::transform(newStr, newStr.begin(), ::toupper);
    return newStr;
}

quark::PSString::PSString() = default;

quark::PSString::PSString(std::string stdString): stringValue(std::move(stdString))
{
}

quark::PSString& quark::PSString::operator=(const PSString& other) = default;
