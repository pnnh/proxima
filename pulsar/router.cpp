#include "router.h"

#include "quark/core/string//string.h"
#include <iostream>
#include <regex>

#include "controllers/article.h"
#include "controllers/channel.h"
#include "controllers/filesystem/filesystem.h"
#include "controllers/index.h"
#include "controllers/sitemap.h"
#include <quark/infra/http/query.h>

void routeHandleGet(WFHttpTask *httpTask, const std::string &request_uri) {
  auto queryParam = quark::MTQueryString{request_uri};
  auto routePath = queryParam.getPath();
  if (quark::MTString::StartsWith(request_uri, "/sitemap")) {
    pulsar::HandleSitemap(httpTask);
  } else if (quark::MTString::StartsWith(request_uri, "/channels")) {
    pulsar::HandleChannels(httpTask);
  } else if (quark::MTString::StartsWith(request_uri, "/files")) {
    pulsar::HandleFileList(httpTask);
  } else if (quark::MTString::StartsWith(request_uri, "/")) {
    pulsar::HandleIndex(httpTask);
  } else {
    protocol::HttpResponse *response = httpTask->get_resp();
    response->set_status_code("404");
  }
}

int pulsar::PLRoute::Match(WFHttpTask *httpTask,
                           std::shared_ptr<PLRouteContext> routeContext) {
  protocol::HttpRequest *request = httpTask->get_req();
  std::string request_uri = request->get_request_uri();
  std::string request_method = request->get_method();
  auto queryParam = quark::MTQueryString{request_uri};
  auto queryPath = queryParam.getPath();
  if (this->method != request_method) {
    return 0; // 方法不匹配
  }
  // if (this->routePath == queryPath) {
  //   return 1; // 完全匹配，最高优先级
  // }
  return parseRouteParams(queryPath, routeContext); // 匹配参数
}

// 匹配单个路由目录，返回一个代表优先级的16进制字符
char pulsar::PLRoute::matchDir(const std::string &routeDir,
                               const std::string &queryDir,
                               std::shared_ptr<PLRouteContext> routeContext) {
  // 匹配到路由参数
  if (quark::MTString::StartsWith(routeDir, ":")) {
    auto paramName = routeDir.substr(1);
    routeContext->setParams(paramName, queryDir);
    return '2'; // 参数
  }
  if (routeDir == queryDir) {
    return '1'; // 最佳匹配
  }
  return '0';
}

std::string pulsar::PLRouteContext::getParams(const std::string &key) {
  return this->routeParams[key];
}

void pulsar::PLRouteContext::setParams(const std::string &key,
                                       const std::string &value) {
  this->routeParams[key] = value;
}

int pulsar::PLRoute::parseRouteParams(
    const std::string &queryPath,
    std::shared_ptr<PLRouteContext> routeContext) {
  auto routePath = this->routePath;
  auto newRoutePath = quark::MTString::trimAll(routePath, "/");
  auto routeSplit = quark::MTString::SplitString(newRoutePath, "/");

  auto newQueryPath = quark::MTString::trimAll(queryPath, "/");
  auto querySplit = quark::MTString::SplitString(newQueryPath, "/");
  if (routeSplit.size() != querySplit.size()) {
    return 0; // 不匹配
  }

  std::string matchPriorityStr;
  for (auto index = 0; index < routeSplit.size(); index++) {
    const auto &routeDir = routeSplit[index];
    const auto &queryDir = querySplit[index];

    if (routeDir.empty() || queryDir.empty()) {
      return 0;
    }
    auto dirMatchChar = matchDir(routeDir, queryDir, routeContext);
    if (dirMatchChar == '0') {
      return 0; // 不匹配
    }
    matchPriorityStr += dirMatchChar;
  }

  int priority = std::stoi(matchPriorityStr, nullptr, 16);

  return priority;
}

void pulsar::PLRouter::Reg(
    std::string method, const std::string &path,
    void (*handler)(WFHttpTask *httpTask,

                    std::shared_ptr<PLRouteContext> routeContext)) {
  PLRoute route;
  route.routePath = path;
  route.handler = handler;
  route.method = std::move(method);
  route.priority = 0;
  routes[path] = route;
}

void pulsar::PLRouter::Route(WFHttpTask *httpTask) {
  protocol::HttpRequest *request = httpTask->get_req();
  std::string request_uri = request->get_request_uri();
  std::string request_method = request->get_method();

  int currentPriority = 9999;
  PLRoute *currentRoute = nullptr;
  std::map<std::string, std::shared_ptr<PLRouteContext>> routeContextMap;
  for (auto &route : routes) {
    auto tempRouteContext = std::make_shared<PLRouteContext>();
    routeContextMap[route.first] = tempRouteContext;
    int matchPriority = route.second.Match(httpTask, tempRouteContext);
    if (matchPriority > 0 && matchPriority < currentPriority) {
      currentPriority = matchPriority;
      currentRoute = &route.second;
    }
  }
  if (currentRoute != nullptr) {
    auto routeContext = routeContextMap[currentRoute->routePath];
    if (routeContextMap.find(currentRoute->routePath) !=
        routeContextMap.end()) {
      currentRoute->handler(httpTask, routeContext);
    }
  } else if (request_method == "GET") {
    routeHandleGet(httpTask, request_uri);
  } else {
    protocol::HttpResponse *response = httpTask->get_resp();
    response->set_status_code("404");
  }
}