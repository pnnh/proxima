#include "appconfig.h"

#include <iostream>
#include <quark/services/yaml/yaml.h>
#include <spdlog/spdlog.h>

pulsar::AppConfig::AppConfig() : yamlHandler("config.yaml") {}

std::string pulsar::AppConfig::GetDSN() {
  return yamlHandler.getString("DATABASE");
}

std::string pulsar::AppConfig::GetSourcePath() {
  std::string sourcePath = yamlHandler.getString("SOURCE_PATH");
  if (sourcePath.empty()) {
    throw std::runtime_error("SOURCE_PATH is empty");
  }
  return sourcePath;
}

std::string pulsar::AppConfig::GetTargetPath() {
  auto targetPath = yamlHandler.getString("TARGET_PATH");
  if (targetPath.empty()) {
    throw std::runtime_error("TARGET_PATH is empty");
  }
  return targetPath;
}
