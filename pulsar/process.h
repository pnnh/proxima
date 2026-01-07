#pragma once
#include "router.h"

#include <workflow/WFHttpServer.h>

namespace pulsar {

class PLServer {
public:
  explicit PLServer(int port);
  int runServer();
  void Reg(std::string method, const std::string &path,
           void (*handler)(WFHttpTask *,
                           std::shared_ptr<PLRouteContext> routeContext));
  void Route(WFHttpTask *httpTask);

private:
  void process(WFHttpTask *httpTask);

  int port;
  PLRouter router;
  std::unique_ptr<WFHttpServer> httpServer;
};
} // namespace pulsar
