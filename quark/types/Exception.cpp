#include "Exception.h"

quark::PSException::PSException() : _codeEnum(QuantumEnum::ERROR)
{
}

quark::PSException::PSException(const std::string& exceptionMessage) : _codeEnum(QuantumEnum::ERROR),
    _exceptionMessage(exceptionMessage)
{
}

quark::PSException::PSException(const char* firstMessage, const char* secondMessage) :
    _codeEnum(QuantumEnum::ERROR),
    _exceptionMessage(firstMessage)
{
    _exceptionMessage = _exceptionMessage.value_or("") + secondMessage;
}

quark::PSException::PSException(const PSException& other) : exception(other)
{
    _codeEnum = other._codeEnum;
    _exceptionMessage = other._exceptionMessage;
}

quark::PSException& quark::PSException::operator=(const PSException& other)
{
    _codeEnum = other._codeEnum;
    _exceptionMessage = other._exceptionMessage;
    return *this;
}

quark::PSException::PSException(PSException&& other) noexcept : exception(other)
{
    _codeEnum = other._codeEnum;
    _exceptionMessage = other._exceptionMessage;
}

quark::PSException& quark::PSException::operator=(PSException&& other) noexcept
{
    _codeEnum = other._codeEnum;
    _exceptionMessage = other._exceptionMessage;
    return *this;
}

quark::PSException::PSException(QuantumEnum error, std::string exceptionMessage) : _codeEnum(error),
    _exceptionMessage(exceptionMessage)
{
}

void quark::PSException::AppendMessage(const std::string& exceptionMessage)
{
    this->_exceptionMessage = this->_exceptionMessage.value_or("") + exceptionMessage;
}
