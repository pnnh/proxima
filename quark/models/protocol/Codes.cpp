
#include "Codes.h"

const char* quark::CodeMessage(int code)
{
  switch (code)
  {
  case quark::Codes::Ok:
    return "Ok";
  case quark::Codes::Error:
    return "Error";
  default:
    return "Unknown";
  }
}
