#pragma once

#include <workflow/WFTaskFactory.h>

namespace pulsar {

class PLRouteContext {
public:
  std::string getParams(const std::string &key);
  void setParams(const std::string &key, const std::string &value);

private:
  std::map<std::string, std::string> routeParams;
};

class PLRoute {
public:
  std::string routePath;
  void (*handler)(WFHttpTask *, std::shared_ptr<PLRouteContext> routeContext);
  std::string method;
  int priority;
  int Match(WFHttpTask *httpTask, std::shared_ptr<PLRouteContext> routeContext);
  int parseRouteParams(const std::string &queryPath,
                       std::shared_ptr<PLRouteContext> routeContext);
  char matchDir(const std::string &routeDir, const std::string &queryDir,
                std::shared_ptr<PLRouteContext> routeContext);
};

class PLRouter {
public:
  PLRouter() = default;
  void Reg(std::string method, const std::string &path,
           void (*handler)(WFHttpTask *,
                           std::shared_ptr<PLRouteContext> routeContext));
  void Route(WFHttpTask *httpTask);

private:
  std::map<std::string, PLRoute> routes;
};
} // namespace pulsar
