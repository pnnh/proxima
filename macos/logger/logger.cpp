#include "logger.h"

#include <iostream>

void kepler::Logger::LogInfo(const std::string& message)
{
    std::cout << "[INFO] " << message << std::endl;
}
