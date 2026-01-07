#pragma once

#include <chrono>
#include <string>

struct MTMailModel {
    std::string uid;
    std::string title;
    std::string content;
    std::chrono::system_clock::time_point create_time;
    std::chrono::system_clock::time_point update_time;
    std::string creator;
    std::string sender;
    std::string receiver;
};

