#pragma once

#include <quark/services/yaml/yaml.h>
#include <string>

namespace pulsar {
class AppConfig {
public:
  static AppConfig &Default() {
    static AppConfig instance;
    return instance;
  }

  AppConfig();
  std::string GetDSN();
  std::string GetSourcePath();
  std::string GetTargetPath();

private:
  quark::YamlHandler yamlHandler;
};
} // namespace pulsar
