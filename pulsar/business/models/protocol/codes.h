#pragma once

namespace pulsar {
enum codes { Ok = 200, Error = 500 };

const char *CodeMessage(int code);
} // namespace pulsar