
#include "codes.h"

const char *pulsar::CodeMessage(int code) {
  switch (code) {
  case pulsar::codes::Ok:
    return "Ok";
  case pulsar::codes::Error:
    return "Error";
  default:
    return "Unknown";
  }
}
